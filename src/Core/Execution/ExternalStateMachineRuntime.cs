using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Introspection;

#pragma warning disable CS8714

namespace StateMachineLibrary.Core.Execution;

/// <summary>Runtime context that reads and writes active state through application-owned accessors.</summary>
public sealed class ExternalStateMachineRuntime<TState, TEvent> : IAsyncDisposable
{
    private readonly CompletionEpisodeTracker<TState> _completionEpisodes = new();
    private readonly SerializedTransitionGate? _gate;
    private readonly Dictionary<TState, CompositeHistoryRecord<TState>> _historyRecords = [];
    private readonly ITransitionObserver<TState, TEvent>? _observer;
    private readonly ParallelHistoryStore<TState> _parallelHistoryStore = new();
    private ActiveStateShape<TState>? _activeStateShape;

    internal ExternalStateMachineRuntime(
        StateMachineDefinition<TState, TEvent> definition,
        IStateAccessor<TState> accessor,
        ConcurrencyMode concurrencyMode,
        ITransitionObserver<TState, TEvent>? observer = null)
    {
        Definition = definition;
        StateAccessor = accessor;
        ConcurrencyMode = concurrencyMode;
        _observer = observer;
        _gate = concurrencyMode == ConcurrencyMode.Serialized ? new SerializedTransitionGate() : null;
    }

    public StateMachineDefinition<TState, TEvent> Definition { get; }
    public IStateAccessor<TState> StateAccessor { get; }
    public ConcurrencyMode ConcurrencyMode { get; }
    public IReadOnlyList<CompositeHistorySnapshot<TState>> HistorySnapshots => CreateHistorySnapshots();

    public IReadOnlyList<ParallelHistorySnapshot<TState>> ParallelHistorySnapshots =>
        _parallelHistoryStore.CreateSnapshots(Definition);

    public async ValueTask DisposeAsync()
    {
        if (_gate is not null) await _gate.DisposeAsync().ConfigureAwait(false);
    }

    public async ValueTask<TState> GetCurrentStateAsync(CancellationToken cancellationToken = default)
    {
        return await StateAccessor.GetAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<ActiveStateShape<TState>> GetActiveStateShapeAsync(
        CancellationToken cancellationToken = default)
    {
        var current = await StateAccessor.GetAsync(cancellationToken).ConfigureAwait(false);
        EnsureActiveStateShape(current);
        return _activeStateShape!;
    }

    /// <summary>Captures the accessor-backed runtime active-state shape without transition side effects.</summary>
    public async ValueTask<ActiveStateSnapshot<TState>> CaptureActiveStateSnapshotAsync(
        string? definitionFingerprint = null,
        CancellationToken cancellationToken = default)
    {
        var current = await StateAccessor.GetAsync(cancellationToken).ConfigureAwait(false);
        EnsureActiveStateShape(current);
        return _activeStateShape!.ToActiveStateSnapshot(Definition,
            definitionFingerprint ?? ResolveDefinitionFingerprint());
    }

    public async ValueTask<TransitionOutcome<TState, TEvent>> ApplyAsync(TEvent @event,
        CancellationToken cancellationToken = default)
    {
        if (_gate is null) return await ApplyCoreAsync(@event, cancellationToken).ConfigureAwait(false);

        using var lease = await _gate.EnterAsync(cancellationToken).ConfigureAwait(false);
        return await ApplyCoreAsync(@event, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<EventDefinition<TEvent>>> GetPermittedEventsAsync(
        CancellationToken cancellationToken = default)
    {
        var state = await StateAccessor.GetAsync(cancellationToken).ConfigureAwait(false);
        return await PermittedEventQuery.GetPermittedEventsAsync(Definition, state, cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask<TransitionOutcome<TState, TEvent>> ApplyCoreAsync(TEvent @event,
        CancellationToken cancellationToken)
    {
        var current = await StateAccessor.GetAsync(cancellationToken).ConfigureAwait(false);
        EnsureActiveStateShape(current);

        if (_activeStateShape!.IsParallel)
            return await ApplyParallelCoreAsync(@event, cancellationToken).ConfigureAwait(false);

        var outcome = await new TransitionExecutor<TState, TEvent>(Definition)
            .ApplyAsync(current, @event, cancellationToken, _observer, _historyRecords, _parallelHistoryStore,
                _activeStateShape)
            .ConfigureAwait(false);

        if (!outcome.Committed) return outcome;

        var before = _activeStateShape!;
        await CommitParentOutcomeAsync(outcome, cancellationToken).ConfigureAwait(false);
        _completionEpisodes.ResetExitedScopes(Definition, before, _activeStateShape!);
        var normalized = NormalizeCommittedOutcome(outcome,
            _activeStateShape!.IsParallel ? _activeStateShape.ActiveRegions[0].ActiveLeafState : outcome.ResultingState,
            _activeStateShape);
        return await RunCompletionCascadeAsync(normalized, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<TransitionOutcome<TState, TEvent>> ApplyParallelCoreAsync(TEvent @event,
        CancellationToken cancellationToken)
    {
        var validation = Definition.Validate();
        var currentLeaf = _activeStateShape!.ActiveRegions[0].ActiveLeafState;
        if (!validation.IsValid)
            return TransitionOutcome<TState, TEvent>.ValidationFailure(currentLeaf, @event,
                new TransitionDiagnostics("Machine definition has validation errors.",
                    validationFindings: validation.Errors,
                    conflictDiagnostics: validation.ConflictDiagnostics));

        var owner = _activeStateShape.OwningCompositeState!;
        var parentTransition = new TransitionMatcher<TState, TEvent>(Definition).Match(owner, @event);
        var matches = new ParallelTransitionResolver<TState, TEvent>(Definition).Resolve(_activeStateShape, @event);
        if (matches.Count == 0)
        {
            if (parentTransition is null)
                return TransitionOutcome<TState, TEvent>.NotPermitted(currentLeaf, @event,
                    new TransitionDiagnostics($"Event '{@event}' is not permitted from active parallel regions.",
                        TransitionLifecyclePhase.Matching));

            await RunParallelExitActionsAsync(parentTransition, @event, cancellationToken).ConfigureAwait(false);
            var parentOutcome = await new TransitionExecutor<TState, TEvent>(Definition)
                .ApplyAsync(owner, @event, cancellationToken, _observer, _historyRecords, _parallelHistoryStore,
                    _activeStateShape)
                .ConfigureAwait(false);
            if (!parentOutcome.Committed) return parentOutcome;

            var before = _activeStateShape!;
            var committedParent = await CommitParentOutcomeAsync(parentOutcome, cancellationToken).ConfigureAwait(false);
            _completionEpisodes.ResetExitedScopes(Definition, before, _activeStateShape!);
            return await RunCompletionCascadeAsync(committedParent, cancellationToken).ConfigureAwait(false);
        }

        var transitions = matches.Select(match => match.Transition).Distinct().ToArray();
        var plannedEntries = PlanPostRegionalEntries(matches);
        var plannedShape = ActiveStateShape<TState>.Parallel(owner, plannedEntries, _activeStateShape.Sequence + 1);
        var parentIsCompletion = parentTransition is not null &&
                                 ParallelCompletionEvaluator.IsComplete(Definition, plannedShape);
        var conflicts = ParallelConflictDetector.Detect(Definition, transitions, parentTransition, parentIsCompletion, @event);
        if (conflicts.Count > 0)
            return TransitionOutcome<TState, TEvent>.ValidationFailure(currentLeaf, @event, conflicts[0]);

        var tempHistory = new Dictionary<TState, CompositeHistoryRecord<TState>>(_historyRecords);
        var entries = _activeStateShape.ActiveRegions.ToDictionary(entry => entry.RegionId, StringComparer.Ordinal);
        TransitionOutcome<TState, TEvent>? firstSuccess = null;

        foreach (var (region, _) in matches)
        {
            var outcome = await new TransitionExecutor<TState, TEvent>(Definition)
                .ApplyAsync(region.ActiveLeafState, @event, cancellationToken, _observer, tempHistory)
                .ConfigureAwait(false);
            if (!outcome.Committed) return outcome;

            var regionDefinition = Definition.GetParallelRegions(owner)
                .First(definition => definition.RegionId == region.RegionId);
            entries[region.RegionId] = new ActiveRegionEntry<TState>(
                region.RegionId,
                region.RegionName,
                outcome.ResultingState,
                Definition.GetActiveStatePath(outcome.ResultingState),
                regionDefinition.TerminalStates.Contains(outcome.ResultingState, EqualityComparer<TState>.Default));
            firstSuccess ??= outcome;
        }

        var ordered = Definition.GetParallelRegions(owner).Select(region => entries[region.RegionId]).ToArray();
        var nextShape = ActiveStateShape<TState>.Parallel(owner, ordered, _activeStateShape.Sequence + 1);

        if (parentTransition is not null && ParallelCompletionEvaluator.IsComplete(Definition, nextShape))
        {
            await RunParallelExitActionsAsync(parentTransition, @event, cancellationToken).ConfigureAwait(false);
            var parentOutcome = await new TransitionExecutor<TState, TEvent>(Definition)
                .ApplyAsync(owner, @event, cancellationToken, _observer, tempHistory, _parallelHistoryStore,
                    _activeStateShape)
                .ConfigureAwait(false);
            if (!parentOutcome.Committed) return parentOutcome;

            var beforeParentCompletion = _activeStateShape!;
            CommitHistory(tempHistory);
            _parallelHistoryStore.RecordChanged(Definition, _activeStateShape, nextShape);
            var committedParent = await CommitParentOutcomeAsync(parentOutcome, cancellationToken).ConfigureAwait(false);
            _completionEpisodes.ResetExitedScopes(Definition, beforeParentCompletion, _activeStateShape!);
            return await RunCompletionCascadeAsync(committedParent, cancellationToken).ConfigureAwait(false);
        }

        CommitHistory(tempHistory);
        _parallelHistoryStore.RecordChanged(Definition, _activeStateShape, nextShape);
        var beforeRegionalShape = _activeStateShape!;
        _activeStateShape = nextShape;
        var persistedState = ordered[0].ActiveLeafState;
        await StateAccessor.SetAsync(persistedState, cancellationToken).ConfigureAwait(false);
        _completionEpisodes.ResetExitedScopes(Definition, beforeRegionalShape, _activeStateShape);
        var regionalOutcome = TransitionOutcome<TState, TEvent>.Success(
            firstSuccess!.PreviousState,
            persistedState,
            @event,
            firstSuccess.Transition!,
            Definition.GetActiveStatePath(persistedState),
            firstSuccess.HistorySnapshots,
            _activeStateShape,
            transitions);
        return await RunCompletionCascadeAsync(regionalOutcome, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<TransitionOutcome<TState, TEvent>> RunCompletionCascadeAsync(
        TransitionOutcome<TState, TEvent> initialOutcome,
        CancellationToken cancellationToken)
    {
        var lastOutcome = initialOutcome;
        while (true)
        {
            var completionOutcome = await TryRunSingleCompletionAsync(cancellationToken).ConfigureAwait(false);
            if (completionOutcome is null) return lastOutcome;

            lastOutcome = completionOutcome;
            if (!completionOutcome.Committed) return lastOutcome;
        }
    }

    private async ValueTask<TransitionOutcome<TState, TEvent>?> TryRunSingleCompletionAsync(
        CancellationToken cancellationToken)
    {
        var selector = new CompletionTransitionSelector<TState, TEvent>(Definition);
        TState completionScope;
        TState executorState;
        var isParallelCompletion = false;

        if (_activeStateShape!.IsParallel)
        {
            if (!ParallelCompletionEvaluator.IsComplete(Definition, _activeStateShape) ||
                _activeStateShape.OwningCompositeState is null)
                return null;

            completionScope = _activeStateShape.OwningCompositeState;
            executorState = completionScope;
            isParallelCompletion = true;
        }
        else
        {
            var completion = HierarchyCompletionEvaluator.Evaluate(Definition, _activeStateShape);
            if (completion is null || completion.Kind == NestedCompletionKind.Machine) return null;

            completionScope = completion.CompletionScopeState!;
            executorState = _activeStateShape.ActiveLeafState!;
        }

        if (_completionEpisodes.IsRecognized(completionScope) || !selector.HasCandidates(completionScope))
            return null;

        var selection = await selector.SelectWithDiagnosticsAsync(executorState, completionScope, cancellationToken)
            .ConfigureAwait(false);
        if (selection.IsAmbiguous)
            return TransitionOutcome<TState, TEvent>.ValidationFailure(_activeStateShape.ActiveLeafState!, default!,
                new TransitionDiagnostics(selection.ConflictDiagnostic!.Message, TransitionLifecyclePhase.Matching,
                    conflictDiagnostics: [selection.ConflictDiagnostic]));

        if (selection.Selected is null)
        {
            _completionEpisodes.MarkNoEligible(completionScope);
            return null;
        }

        var selected = selection.Selected;
        _completionEpisodes.MarkSelected(completionScope);
        var before = _activeStateShape;
        if (isParallelCompletion)
            await RunParallelExitActionsAsync(selected.ToExecutableTransition(false), default!, cancellationToken)
                .ConfigureAwait(false);

        var outcome = await new TransitionExecutor<TState, TEvent>(Definition)
            .ApplyCompletionAsync(executorState, selected, cancellationToken, _observer, _historyRecords,
                _parallelHistoryStore, before)
            .ConfigureAwait(false);

        if (outcome.Committed)
        {
            await CommitParentOutcomeAsync(outcome, cancellationToken).ConfigureAwait(false);
            _completionEpisodes.ResetExitedScopes(Definition, before, _activeStateShape!);
            return NormalizeCommittedOutcome(outcome,
                _activeStateShape!.IsParallel ? _activeStateShape.ActiveRegions[0].ActiveLeafState : outcome.ResultingState,
                _activeStateShape);
        }

        return outcome;
    }

    private IReadOnlyList<ActiveRegionEntry<TState>> PlanPostRegionalEntries(
        IReadOnlyList<(ActiveRegionEntry<TState> Region, TransitionDefinition<TState, TEvent> Transition)> matches)
    {
        var entries = _activeStateShape!.ActiveRegions.ToDictionary(entry => entry.RegionId, StringComparer.Ordinal);
        foreach (var (region, transition) in matches)
        {
            var target = transition.Kind == TransitionKind.Internal
                ? region.ActiveLeafState
                : InitialChildResolver.ResolveTargetLeaf(Definition, transition.TargetState);
            var regionDefinition = Definition.GetParallelRegions(_activeStateShape.OwningCompositeState!)
                .First(definition => definition.RegionId == region.RegionId);
            entries[region.RegionId] = new ActiveRegionEntry<TState>(
                region.RegionId,
                region.RegionName,
                target,
                Definition.GetActiveStatePath(target),
                regionDefinition.TerminalStates.Contains(target, EqualityComparer<TState>.Default));
        }

        return Definition.GetParallelRegions(_activeStateShape.OwningCompositeState!)
            .Select(region => entries[region.RegionId]).ToArray();
    }

    private async ValueTask<TransitionOutcome<TState, TEvent>> CommitParentOutcomeAsync(
        TransitionOutcome<TState, TEvent> parentOutcome, CancellationToken cancellationToken)
    {
        var target = parentOutcome.Transition is null
            ? parentOutcome.ResultingState
            : parentOutcome.Transition.TargetState;
        _activeStateShape = Definition.IsParallelComposite(target)
            ? EnterParallelTarget(target, _activeStateShape)
            : ActiveStateShape<TState>.Single(parentOutcome.ResultingState, (_activeStateShape?.Sequence ?? 0) + 1);

        if (_activeStateShape.IsParallel) _parallelHistoryStore.Record(Definition, _activeStateShape);

        var persistedState = _activeStateShape.IsParallel
            ? _activeStateShape.ActiveRegions[0].ActiveLeafState
            : parentOutcome.ResultingState;
        await StateAccessor.SetAsync(persistedState, cancellationToken).ConfigureAwait(false);
        return NormalizeCommittedOutcome(parentOutcome, persistedState, _activeStateShape);
    }

    private void EnsureActiveStateShape(TState currentState)
    {
        if (_activeStateShape is null)
        {
            _activeStateShape = Definition.IsParallelComposite(currentState)
                ? EnterParallelTarget(currentState, null)
                : ActiveStateShape<TState>.Single(Definition.HasHierarchy
                    ? InitialChildResolver.ResolveTargetLeaf(Definition, currentState)
                    : currentState);
            return;
        }

        var comparer = EqualityComparer<TState>.Default;
        if (_activeStateShape.IsParallel)
        {
            var currentLeaf = _activeStateShape.ActiveRegions[0].ActiveLeafState;
            if (!comparer.Equals(currentLeaf, currentState))
                _activeStateShape = Definition.IsParallelComposite(currentState)
                    ? EnterParallelTarget(currentState, _activeStateShape)
                    : ActiveStateShape<TState>.Single(
                        Definition.HasHierarchy
                            ? InitialChildResolver.ResolveTargetLeaf(Definition, currentState)
                            : currentState, _activeStateShape.Sequence + 1);

            return;
        }

        if (!comparer.Equals(_activeStateShape.ActiveLeafState!, currentState))
            _activeStateShape = ActiveStateShape<TState>.Single(
                Definition.HasHierarchy
                    ? InitialChildResolver.ResolveTargetLeaf(Definition, currentState)
                    : currentState, _activeStateShape.Sequence + 1);
    }

    private ActiveStateShape<TState> EnterParallelTarget(TState target, ActiveStateShape<TState>? preRestoreShape)
    {
        if (Definition.TryGetHistoryDefinition(target, out var state) && state.HistoryMode != HistoryMode.None)
        {
            var plan = new ParallelHistoryRestorePlanner<TState>().Plan(Definition, target, _parallelHistoryStore,
                preRestoreShape);
            if (plan.IsValid) return plan.PlannedPostRestoreShape;
        }

        return ParallelRegionInitialResolver.Enter(Definition, target, (preRestoreShape?.Sequence ?? 0) + 1);
    }

    private TransitionOutcome<TState, TEvent> NormalizeCommittedOutcome(TransitionOutcome<TState, TEvent> outcome,
        TState persistedState, ActiveStateShape<TState> activeShape)
    {
        if (!outcome.IsSuccess || outcome.Transition is null) return outcome;

        return TransitionOutcome<TState, TEvent>.Success(
            outcome.PreviousState,
            persistedState,
            outcome.Event,
            outcome.Transition,
            Definition.GetActiveStatePath(persistedState),
            outcome.HistorySnapshots,
            activeShape,
            outcome.ParallelTransitions);
    }

    private void CommitHistory(Dictionary<TState, CompositeHistoryRecord<TState>> tempHistory)
    {
        _historyRecords.Clear();
        foreach (var item in tempHistory) _historyRecords[item.Key] = item.Value;
    }

    private async ValueTask RunParallelExitActionsAsync(TransitionDefinition<TState, TEvent> parentTransition,
        TEvent @event, CancellationToken cancellationToken)
    {
        var runner = new TransitionActionRunner<TState, TEvent>();
        foreach (var region in HierarchyEntryExitPlanner.ExitOrder(_activeStateShape!.ActiveRegions))
        {
            var context = new ActionExecutionContext<TState, TEvent>(Definition, region.ActiveLeafState,
                parentTransition.TargetState, @event, parentTransition, TransitionLifecyclePhase.Exit,
                cancellationToken, regionId: region.RegionId, regionName: region.RegionName,
                triggerKind: parentTransition.TriggerKind);
            foreach (var state in region.ActivePath.StatesRootToLeaf.Reverse().TakeWhile(state =>
                         !EqualityComparer<TState>.Default.Equals(state, _activeStateShape.OwningCompositeState)))
                await runner.RunStateActionsAsync(Definition.FindState(state), ActionKind.Exit, parentTransition,
                    context, cancellationToken).ConfigureAwait(false);
        }
    }

    private string? ResolveDefinitionFingerprint()
    {
        return Definition.Metadata.TryGetValue(StateMachineMetadataKeys.DefinitionFingerprint, out var value)
            ? Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)
            : null;
    }

    private IReadOnlyList<CompositeHistorySnapshot<TState>> CreateHistorySnapshots()
    {
        return Definition.HistoryEnabledStates
            .Select(state =>
            {
                _historyRecords.TryGetValue(state.Value, out var record);
                Definition.TryGetEffectiveHistoryFallback(state.Value, out var fallback);
                return new CompositeHistorySnapshot<TState>(
                    state.Value,
                    record is not null,
                    record is null ? default : record.LastActiveDirectChildState,
                    record is null ? default : record.LastActiveDescendantLeafState,
                    fallback,
                    record?.LastUpdatedSequence ?? 0);
            })
            .ToArray();
    }
}

#pragma warning restore CS8714