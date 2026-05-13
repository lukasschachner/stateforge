using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Diagnostics;
using StateMachineLibrary.Core.Introspection;

#pragma warning disable CS8714

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

    /// <summary>Exports definition graph data with runtime active-state overlay metadata.</summary>
    public GraphExportResult<TState, TEvent> ExportGraph()
    {
        return ExportGraph(null);
    }

    /// <summary>Exports definition graph data with runtime overlay behavior controlled by <paramref name="options"/>.</summary>
    public GraphExportResult<TState, TEvent> ExportGraph(RuntimeGraphExportOptions? options)
    {
        return DefinitionGraphExporter.ExportGraph(Definition, ActiveStateShape, options);
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

    /// <summary>
    /// Previews an event from the runtime's current active shape without committing state or invoking observers.
    /// </summary>
    public async ValueTask<TransitionPreviewResult<TState, TEvent>> PreviewAsync(TEvent @event,
        CancellationToken cancellationToken = default)
    {
        if (_gate is null) return await PreviewCoreAsync(@event, cancellationToken).ConfigureAwait(false);

        using var lease = await _gate.EnterAsync(cancellationToken).ConfigureAwait(false);
        return await PreviewCoreAsync(@event, cancellationToken).ConfigureAwait(false);
    }

    public ValueTask<IReadOnlyList<EventDefinition<TEvent>>> GetPermittedEventsAsync(
        CancellationToken cancellationToken = default)
    {
        return PermittedEventQuery.GetPermittedEventsAsync(Definition, ActiveStateShape, cancellationToken);
    }

    private ValueTask<TransitionPreviewResult<TState, TEvent>> PreviewCoreAsync(TEvent @event,
        CancellationToken cancellationToken)
    {
        return new TransitionPreviewPlanner<TState, TEvent>(Definition).PreviewAsync(ActiveStateShape, @event,
            cancellationToken, _historyRecords, _parallelHistoryStore);
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
                    validationFindings: validation.Errors,
                    conflictDiagnostics: validation.ConflictDiagnostics,
                    denialDiagnostics:
                    [
                        TransitionDenialDiagnostic.ValidationConflicts(validation.Errors,
                            validation.ConflictDiagnostics)
                    ]));

        var owner = ActiveStateShape.OwningCompositeState!;
        var parentTransition = new TransitionMatcher<TState, TEvent>(Definition).Match(owner, @event);
        var matches = new ParallelTransitionResolver<TState, TEvent>(Definition).Resolve(ActiveStateShape, @event);
        if (matches.Count == 0)
        {
            if (parentTransition is null)
                return TransitionOutcome<TState, TEvent>.NotPermitted(CurrentState, @event,
                    new TransitionDiagnostics($"Event '{@event}' is not permitted from active parallel regions.",
                        TransitionLifecyclePhase.Matching,
                        denialDiagnostics:
                        [
                            TransitionDenialDiagnostic.NoMatchingEvent(CurrentState, ResolveEventIdentity(@event))
                        ]));

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
        var plannedEntries = RuntimeTransitionHelpers.PlanPostRegionalEntries(Definition, ActiveStateShape, matches);
        var plannedShape = ActiveStateShape<TState>.Parallel(owner, plannedEntries, ActiveStateShape.Sequence + 1);
        var parentIsCompletion = parentTransition is not null &&
                                 ParallelCompletionEvaluator.IsComplete(Definition, plannedShape);
        var conflicts = ParallelConflictDetector.Detect(Definition, transitions, parentTransition, parentIsCompletion, @event);
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

        var selection = await selector.SelectWithDiagnosticsAsync(executorState, completionScope, cancellationToken)
            .ConfigureAwait(false);
        if (selection.IsAmbiguous)
            return TransitionOutcome<TState, TEvent>.ValidationFailure(CurrentState, default!,
                new TransitionDiagnostics(selection.ConflictDiagnostic!.Message, TransitionLifecyclePhase.Matching,
                    conflictDiagnostics: [selection.ConflictDiagnostic]));

        if (selection.Selected is null)
        {
            _completionEpisodes.MarkNoEligible(completionScope);
            return null;
        }

        var selected = selection.Selected;
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
        RuntimeTransitionHelpers.CommitHistory(_historyRecords, tempHistory);
    }

    private async ValueTask RunParallelExitActionsAsync(TransitionDefinition<TState, TEvent> parentTransition,
        TEvent @event, CancellationToken cancellationToken)
    {
        await RuntimeTransitionHelpers.RunParallelExitActionsAsync(Definition, ActiveStateShape, parentTransition,
            @event, cancellationToken).ConfigureAwait(false);
    }

    private string? ResolveEventIdentity(TEvent @event)
    {
        return RuntimeTransitionHelpers.ResolveEventIdentity(Definition, @event);
    }

    private string? ResolveDefinitionFingerprint()
    {
        return Definition.Metadata.TryGetValue(StateMachineMetadataKeys.DefinitionFingerprint, out var value)
            ? Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)
            : null;
    }

    private IReadOnlyList<CompositeHistorySnapshot<TState>> CreateHistorySnapshots()
    {
        return RuntimeTransitionHelpers.CreateHistorySnapshots(Definition, _historyRecords);
    }
}

#pragma warning restore CS8714