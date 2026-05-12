using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Introspection;

namespace StateMachineLibrary.Core.Execution;

/// <summary>State-owning runtime context for a reusable machine definition.</summary>
public sealed class StateMachineRuntime<TState, TEvent> : IAsyncDisposable
{
    private readonly CompletionEpisodeTracker<TState> _completionEpisodes = new();
    private readonly SerializedTransitionGate? _gate;
    private readonly Dictionary<TState, CompositeHistoryRecord<TState>> _historyRecords = [];
    private readonly ITransitionObserver<TState, TEvent>? _observer;
    private readonly ParallelHistoryStore<TState> _parallelHistoryStore = new();

    internal StateMachineRuntime(
        StateMachineDefinition<TState, TEvent> definition,
        TState initialState,
        ConcurrencyMode concurrencyMode,
        ITransitionObserver<TState, TEvent>? observer = null)
        : this(definition,
            definition.IsParallelComposite(initialState)
                ? ParallelRegionInitialResolver.Enter(definition, initialState)
                : ActiveStateShape<TState>.Single(definition.HasHierarchy
                    ? InitialChildResolver.ResolveTargetLeaf(definition, initialState)
                    : initialState),
            concurrencyMode,
            observer)
    {
    }

    internal StateMachineRuntime(
        StateMachineDefinition<TState, TEvent> definition,
        ActiveStateShape<TState> activeStateShape,
        ConcurrencyMode concurrencyMode,
        ITransitionObserver<TState, TEvent>? observer = null)
    {
        Definition = definition;
        ActiveStateShape = activeStateShape;
        CurrentState = ActiveStateShape.IsParallel
            ? ActiveStateShape.ActiveRegions[0].ActiveLeafState
            : ActiveStateShape.ActiveLeafState!;
        ConcurrencyMode = concurrencyMode;
        _observer = observer;
        _gate = concurrencyMode == ConcurrencyMode.Serialized ? new SerializedTransitionGate() : null;
    }

    public StateMachineDefinition<TState, TEvent> Definition { get; }
    public TState CurrentState { get; private set; }
    public ActiveStateShape<TState> ActiveStateShape { get; private set; }

    public ActiveStatePath<TState> ActiveStatePath => ActiveStateShape.IsParallel
        ? ActiveStateShape.ActiveRegions[0].ActivePath
        : Definition.GetActiveStatePath(CurrentState);

    public ConcurrencyMode ConcurrencyMode { get; }
    public IReadOnlyList<CompositeHistorySnapshot<TState>> HistorySnapshots => CreateHistorySnapshots();

    public IReadOnlyList<ParallelHistorySnapshot<TState>> ParallelHistorySnapshots =>
        _parallelHistoryStore.CreateSnapshots(Definition);

    /// <summary>Captures the current active-state shape without executing transition lifecycle behavior.</summary>
    public ActiveStateSnapshot<TState> CaptureActiveStateSnapshot(string? definitionFingerprint = null)
    {
        return ActiveStateShape.ToActiveStateSnapshot(Definition,
            definitionFingerprint ?? ResolveDefinitionFingerprint());
    }

    public async ValueTask DisposeAsync()
    {
        if (_gate is not null) await _gate.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>Applies an event and updates current state when the outcome reaches the commit point.</summary>
    public async ValueTask<TransitionOutcome<TState, TEvent>> ApplyAsync(TEvent @event,
        CancellationToken cancellationToken = default)
    {
        if (_gate is null) return await ApplyCoreAsync(@event, cancellationToken).ConfigureAwait(false);

        using var lease = await _gate.EnterAsync(cancellationToken).ConfigureAwait(false);
        return await ApplyCoreAsync(@event, cancellationToken).ConfigureAwait(false);
    }

    public ValueTask<IReadOnlyList<EventDefinition<TEvent>>> GetPermittedEventsAsync(
        CancellationToken cancellationToken = default)
    {
        return PermittedEventQuery.GetPermittedEventsAsync(Definition, CurrentState, cancellationToken);
    }

    private async ValueTask<TransitionOutcome<TState, TEvent>> ApplyCoreAsync(TEvent @event,
        CancellationToken cancellationToken)
    {
        if (ActiveStateShape.IsParallel)
        {
            var parallelOutcome = await ApplyParallelCoreAsync(@event, cancellationToken).ConfigureAwait(false);
            return parallelOutcome;
        }

        var outcome = await new TransitionExecutor<TState, TEvent>(Definition).ApplyAsync(CurrentState, @event,
                cancellationToken, _observer, _historyRecords, _parallelHistoryStore, ActiveStateShape)
            .ConfigureAwait(false);
        if (outcome.Committed)
        {
            var before = ActiveStateShape;
            CommitParentOutcome(outcome);
            _completionEpisodes.ResetExitedScopes(Definition, before, ActiveStateShape);
            return await RunCompletionCascadeAsync(outcome, cancellationToken).ConfigureAwait(false);
        }

        return outcome;
    }

    private async ValueTask<TransitionOutcome<TState, TEvent>> ApplyParallelCoreAsync(TEvent @event,
        CancellationToken cancellationToken)
    {
        var validation = Definition.Validate();
        if (!validation.IsValid)
            return TransitionOutcome<TState, TEvent>.ValidationFailure(CurrentState, @event,
                new TransitionDiagnostics("Machine definition has validation errors.",
                    validationFindings: validation.Errors));

        var owner = ActiveStateShape.OwningCompositeState!;
        var parentTransition = new TransitionMatcher<TState, TEvent>(Definition).Match(owner, @event);
        var matches = new ParallelTransitionResolver<TState, TEvent>(Definition).Resolve(ActiveStateShape, @event);
        if (matches.Count == 0)
        {
            if (parentTransition is null)
                return TransitionOutcome<TState, TEvent>.NotPermitted(CurrentState, @event,
                    new TransitionDiagnostics($"Event '{@event}' is not permitted from active parallel regions.",
                        TransitionLifecyclePhase.Matching));

            await RunParallelExitActionsAsync(parentTransition, @event, cancellationToken).ConfigureAwait(false);
            var parentOutcome = await new TransitionExecutor<TState, TEvent>(Definition).ApplyAsync(owner, @event,
                    cancellationToken, _observer, _historyRecords, _parallelHistoryStore, ActiveStateShape)
                .ConfigureAwait(false);
            if (parentOutcome.Committed)
            {
                var before = ActiveStateShape;
                CommitParentOutcome(parentOutcome);
                _completionEpisodes.ResetExitedScopes(Definition, before, ActiveStateShape);
                return await RunCompletionCascadeAsync(parentOutcome, cancellationToken).ConfigureAwait(false);
            }

            return parentOutcome;
        }

        var transitions = matches.Select(m => m.Transition).Distinct().ToArray();
        var plannedEntries = PlanPostRegionalEntries(matches);
        var plannedShape = ActiveStateShape<TState>.Parallel(owner, plannedEntries, ActiveStateShape.Sequence + 1);
        var parentIsCompletion = parentTransition is not null &&
                                 ParallelCompletionEvaluator.IsComplete(Definition, plannedShape);
        var conflicts = ParallelConflictDetector.Detect(Definition, transitions, parentTransition, parentIsCompletion);
        if (conflicts.Count > 0)
            return TransitionOutcome<TState, TEvent>.ValidationFailure(CurrentState, @event, conflicts[0]);

        var tempHistory = new Dictionary<TState, CompositeHistoryRecord<TState>>(_historyRecords);
        var entries = ActiveStateShape.ActiveRegions.ToDictionary(e => e.RegionId, StringComparer.Ordinal);
        TransitionOutcome<TState, TEvent>? firstSuccess = null;
        foreach (var (region, _) in matches)
        {
            var outcome = await new TransitionExecutor<TState, TEvent>(Definition)
                .ApplyAsync(region.ActiveLeafState, @event, cancellationToken, _observer, tempHistory)
                .ConfigureAwait(false);
            if (!outcome.Committed) return outcome;

            var regionDefinition = Definition.GetParallelRegions(owner).First(r => r.RegionId == region.RegionId);
            entries[region.RegionId] = new ActiveRegionEntry<TState>(region.RegionId, region.RegionName,
                outcome.ResultingState, Definition.GetActiveStatePath(outcome.ResultingState),
                regionDefinition.TerminalStates.Contains(outcome.ResultingState, EqualityComparer<TState>.Default));
            firstSuccess ??= outcome;
        }

        var ordered = Definition.GetParallelRegions(owner).Select(r => entries[r.RegionId]).ToArray();
        var nextShape = ActiveStateShape<TState>.Parallel(owner, ordered, ActiveStateShape.Sequence + 1);

        if (parentTransition is not null && ParallelCompletionEvaluator.IsComplete(Definition, nextShape))
        {
            await RunParallelExitActionsAsync(parentTransition, @event, cancellationToken).ConfigureAwait(false);
            var parentOutcome = await new TransitionExecutor<TState, TEvent>(Definition).ApplyAsync(owner, @event,
                    cancellationToken, _observer, tempHistory, _parallelHistoryStore, ActiveStateShape)
                .ConfigureAwait(false);
            if (!parentOutcome.Committed) return parentOutcome;

            var beforeParentCompletion = ActiveStateShape;
            CommitHistory(tempHistory);
            _parallelHistoryStore.RecordChanged(Definition, ActiveStateShape, nextShape);
            CommitParentOutcome(parentOutcome);
            _completionEpisodes.ResetExitedScopes(Definition, beforeParentCompletion, ActiveStateShape);
            return await RunCompletionCascadeAsync(parentOutcome, cancellationToken).ConfigureAwait(false);
        }

        CommitHistory(tempHistory);
        _parallelHistoryStore.RecordChanged(Definition, ActiveStateShape, nextShape);
        var beforeRegionalShape = ActiveStateShape;
        ActiveStateShape = nextShape;
        CurrentState = ordered[0].ActiveLeafState;
        _completionEpisodes.ResetExitedScopes(Definition, beforeRegionalShape, ActiveStateShape);
        var regionalOutcome = TransitionOutcome<TState, TEvent>.Success(firstSuccess!.PreviousState, CurrentState, @event,
            firstSuccess.Transition!, Definition.GetActiveStatePath(CurrentState), firstSuccess.HistorySnapshots,
            ActiveStateShape, transitions);
        return await RunCompletionCascadeAsync(regionalOutcome, cancellationToken).ConfigureAwait(false);
    }

    private IReadOnlyList<ActiveRegionEntry<TState>> PlanPostRegionalEntries(
        IReadOnlyList<(ActiveRegionEntry<TState> Region, TransitionDefinition<TState, TEvent> Transition)> matches)
    {
        var entries = ActiveStateShape.ActiveRegions.ToDictionary(e => e.RegionId, StringComparer.Ordinal);
        foreach (var (region, transition) in matches)
        {
            var target = transition.Kind == TransitionKind.Internal
                ? region.ActiveLeafState
                : InitialChildResolver.ResolveTargetLeaf(Definition, transition.TargetState);
            var regionDefinition = Definition.GetParallelRegions(ActiveStateShape.OwningCompositeState!)
                .First(r => r.RegionId == region.RegionId);
            entries[region.RegionId] = new ActiveRegionEntry<TState>(region.RegionId, region.RegionName, target,
                Definition.GetActiveStatePath(target),
                regionDefinition.TerminalStates.Contains(target, EqualityComparer<TState>.Default));
        }

        return Definition.GetParallelRegions(ActiveStateShape.OwningCompositeState!).Select(r => entries[r.RegionId])
            .ToArray();
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

        if (ActiveStateShape.IsParallel)
        {
            if (!ParallelCompletionEvaluator.IsComplete(Definition, ActiveStateShape) ||
                ActiveStateShape.OwningCompositeState is null)
                return null;

            completionScope = ActiveStateShape.OwningCompositeState;
            executorState = completionScope;
            isParallelCompletion = true;
        }
        else
        {
            var completion = HierarchyCompletionEvaluator.Evaluate(Definition, ActiveStateShape);
            if (completion is null || completion.Kind == NestedCompletionKind.Machine) return null;

            completionScope = completion.CompletionScopeState!;
            executorState = CurrentState;
        }

        if (_completionEpisodes.IsRecognized(completionScope) || !selector.HasCandidates(completionScope))
            return null;

        var selected = await selector.SelectAsync(executorState, completionScope, cancellationToken).ConfigureAwait(false);
        if (selected is null)
        {
            _completionEpisodes.MarkNoEligible(completionScope);
            return null;
        }

        _completionEpisodes.MarkSelected(completionScope);
        var before = ActiveStateShape;
        if (isParallelCompletion)
            await RunParallelExitActionsAsync(selected.ToExecutableTransition(false), default!, cancellationToken)
                .ConfigureAwait(false);

        var outcome = await new TransitionExecutor<TState, TEvent>(Definition)
            .ApplyCompletionAsync(executorState, selected, cancellationToken, _observer, _historyRecords,
                _parallelHistoryStore, before)
            .ConfigureAwait(false);

        if (outcome.Committed)
        {
            CommitParentOutcome(outcome);
            _completionEpisodes.ResetExitedScopes(Definition, before, ActiveStateShape);
        }

        return outcome;
    }

    private void CommitParentOutcome(TransitionOutcome<TState, TEvent> parentOutcome)
    {
        var target = parentOutcome.Transition is null
            ? parentOutcome.ResultingState
            : parentOutcome.Transition.TargetState;
        ActiveStateShape = Definition.IsParallelComposite(target)
            ? EnterParallelTarget(target, ActiveStateShape)
            : ActiveStateShape<TState>.Single(parentOutcome.ResultingState, ActiveStateShape.Sequence + 1);
        CurrentState = ActiveStateShape.IsParallel
            ? ActiveStateShape.ActiveRegions[0].ActiveLeafState
            : parentOutcome.ResultingState;
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

    private void CommitHistory(Dictionary<TState, CompositeHistoryRecord<TState>> tempHistory)
    {
        _historyRecords.Clear();
        foreach (var item in tempHistory) _historyRecords[item.Key] = item.Value;
    }

    private async ValueTask RunParallelExitActionsAsync(TransitionDefinition<TState, TEvent> parentTransition,
        TEvent @event, CancellationToken cancellationToken)
    {
        var runner = new TransitionActionRunner<TState, TEvent>();
        foreach (var region in HierarchyEntryExitPlanner.ExitOrder(ActiveStateShape.ActiveRegions))
        {
            var context = new ActionExecutionContext<TState, TEvent>(Definition, region.ActiveLeafState,
                parentTransition.TargetState, @event, parentTransition, TransitionLifecyclePhase.Exit,
                cancellationToken, regionId: region.RegionId, regionName: region.RegionName,
                triggerKind: parentTransition.TriggerKind);
            foreach (var state in region.ActivePath.StatesRootToLeaf.Reverse().TakeWhile(s =>
                         !EqualityComparer<TState>.Default.Equals(s, ActiveStateShape.OwningCompositeState)))
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