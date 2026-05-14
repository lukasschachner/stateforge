using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;
using StateForge.Core.Validation;

namespace StateForge.Core.Execution;

/// <summary>Executes supplied-state transition attempts without owning state.</summary>
internal sealed class TransitionExecutor<TState, TEvent>
{
    private readonly TransitionActionRunner<TState, TEvent> _actionRunner = new();
    private readonly TransitionBehaviorRunner<TState, TEvent> _behaviorRunner = new();
    private readonly ConditionEvaluator<TState, TEvent> _conditionEvaluator;
    private readonly StateMachineDefinition<TState, TEvent> _definition;
    private readonly TransitionMatcher<TState, TEvent> _matcher;

    public TransitionExecutor(StateMachineDefinition<TState, TEvent> definition)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _conditionEvaluator = new ConditionEvaluator<TState, TEvent>(_definition);
        _matcher = new TransitionMatcher<TState, TEvent>(definition);
    }

    public ValueTask<TransitionOutcome<TState, TEvent>> ApplyAsync(
        TState currentState,
        TEvent @event,
        CancellationToken cancellationToken = default,
        ITransitionObserver<TState, TEvent>? observer = null,
        IDictionary<TState, CompositeHistoryRecord<TState>>? historyRecords = null,
        ParallelHistoryStore<TState>? parallelHistoryStore = null,
        ActiveStateShape<TState>? preTransitionActiveShape = null)
    {
        return ApplyCoreAsync(currentState, @event, null, cancellationToken, observer, historyRecords,
            parallelHistoryStore, preTransitionActiveShape);
    }

    public ValueTask<TransitionOutcome<TState, TEvent>> ApplyCompletionAsync(
        TState currentState,
        CompletionTransitionDefinition<TState, TEvent> completionTransition,
        CancellationToken cancellationToken = default,
        ITransitionObserver<TState, TEvent>? observer = null,
        IDictionary<TState, CompositeHistoryRecord<TState>>? historyRecords = null,
        ParallelHistoryStore<TState>? parallelHistoryStore = null,
        ActiveStateShape<TState>? preTransitionActiveShape = null)
    {
        return ApplyCoreAsync(currentState, default!, completionTransition.ToExecutableTransition(false), cancellationToken,
            observer, historyRecords, parallelHistoryStore, preTransitionActiveShape);
    }

    private ValueTask<TransitionOutcome<TState, TEvent>> ApplyCoreAsync(
        TState currentState,
        TEvent @event,
        TransitionDefinition<TState, TEvent>? selectedTransition,
        CancellationToken cancellationToken,
        ITransitionObserver<TState, TEvent>? observer,
        IDictionary<TState, CompositeHistoryRecord<TState>>? historyRecords,
        ParallelHistoryStore<TState>? parallelHistoryStore,
        ActiveStateShape<TState>? preTransitionActiveShape)
    {
        return observer is null
            ? ApplyUnobservedAsync(currentState, @event, selectedTransition, cancellationToken, historyRecords,
                parallelHistoryStore, preTransitionActiveShape)
            : ApplyObservedAsync(currentState, @event, selectedTransition, observer, cancellationToken, historyRecords,
                parallelHistoryStore, preTransitionActiveShape);
    }

    private async ValueTask<TransitionOutcome<TState, TEvent>> ApplyObservedAsync(
        TState currentState,
        TEvent @event,
        TransitionDefinition<TState, TEvent>? selectedTransition,
        ITransitionObserver<TState, TEvent> observer,
        CancellationToken cancellationToken,
        IDictionary<TState, CompositeHistoryRecord<TState>>? historyRecords,
        ParallelHistoryStore<TState>? parallelHistoryStore,
        ActiveStateShape<TState>? preTransitionActiveShape)
    {
        var observations = new TransitionObservationScope<TState, TEvent>(observer, _definition, currentState, @event,
            selectedTransition?.TriggerKind ?? TransitionTriggerKind.Event);
        await observations.StartedAsync(cancellationToken).ConfigureAwait(false);

        var validation = _definition.Validate();
        if (!validation.IsValid)
        {
            var outcome = CreateValidationFailureOutcome(currentState, @event, validation);
            await observations.ObserveAsync(TransitionObservationKind.ValidationFailure, TransitionLifecyclePhase.None,
                null, outcome, false, outcome.Diagnostics, currentState, cancellationToken).ConfigureAwait(false);
            await observations.OutcomeAsync(outcome, cancellationToken).ConfigureAwait(false);
            return outcome;
        }

        TransitionDefinition<TState, TEvent>? transition = null;
        HierarchySelectionDiagnostics? hierarchyDiagnostics = null;
        var currentPhase = TransitionLifecyclePhase.Matching;
        var committed = false;
        var resultingState = currentState;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            transition = selectedTransition ?? _matcher.Match(currentState, @event);
            if (transition is null)
            {
                var outcome = TransitionOutcome<TState, TEvent>.NotPermitted(
                    currentState,
                    @event,
                    new TransitionDiagnostics($"Event '{@event}' is not permitted from state '{currentState}'.",
                        TransitionLifecyclePhase.Matching,
                        denialDiagnostics:
                        [
                            CreateNoMatchDiagnostic(currentState, @event)
                        ]));
                await observations.ObserveAsync(TransitionObservationKind.NotPermitted,
                    TransitionLifecyclePhase.Matching, null, outcome, false, outcome.Diagnostics, currentState,
                    cancellationToken).ConfigureAwait(false);
                await observations.OutcomeAsync(outcome, cancellationToken).ConfigureAwait(false);
                return outcome;
            }

            var targetState = ResolveTargetState(currentState, transition, historyRecords, parallelHistoryStore,
                preTransitionActiveShape, out var historyResolution);
            hierarchyDiagnostics = CreateHierarchyDiagnostics(currentState, transition, targetState, historyResolution);
            var targetPath = _definition.GetActiveStatePath(targetState);
            var hierarchyPlan = CreateHierarchyPlan(currentState, targetState, transition);
            var contextSource = transition.IsCompletionTriggered ? transition.SourceState : currentState;
            var context = new TransitionContext<TState, TEvent>(_definition, contextSource, @event, cancellationToken,
                transition, targetState, triggerKind: transition.TriggerKind);

            currentPhase = TransitionLifecyclePhase.Condition;
            var conditionResult =
                await EvaluateConditionsAsync(transition, context, cancellationToken).ConfigureAwait(false);
            if (!conditionResult.IsAllowed)
            {
                var outcome = TransitionOutcome<TState, TEvent>.Denied(
                    currentState,
                    @event,
                    transition,
                    new TransitionDiagnostics(
                        $"Condition '{conditionResult.DeniedCondition!.DisplayName}' denied the transition.",
                        TransitionLifecyclePhase.Condition,
                        affectedElement: conditionResult.DeniedCondition.DisplayName,
                        hierarchyMetadata: hierarchyDiagnostics,
                        denialDiagnostics:
                        [
                            TransitionDenialDiagnostic.FailedGuards(
                                GetTransitionId(transition),
                                [new TransitionPreviewGuardDiagnostic(
                                    GetTransitionId(transition), 0,
                                    conditionResult.DeniedCondition.DisplayName,
                                    TransitionPreviewGuardStatus.Failed)])
                        ]));
                await observations.ObserveAsync(TransitionObservationKind.ConditionDenied,
                    TransitionLifecyclePhase.Condition, transition, outcome, false, outcome.Diagnostics, currentState,
                    cancellationToken).ConfigureAwait(false);
                await observations.OutcomeAsync(outcome, cancellationToken).ConfigureAwait(false);
                return outcome;
            }

            if (transition.Kind is TransitionKind.External or TransitionKind.Self)
            {
                currentPhase = TransitionLifecyclePhase.Exit;
                await RunPreCommitPhaseAsync(transition, currentPhase, context, hierarchyPlan, cancellationToken)
                    .ConfigureAwait(false);
            }

            currentPhase = TransitionLifecyclePhase.Transition;
            await RunPreCommitPhaseAsync(transition, currentPhase, context, hierarchyPlan, cancellationToken)
                .ConfigureAwait(false);

            if (transition.Kind is TransitionKind.External or TransitionKind.Self)
            {
                currentPhase = TransitionLifecyclePhase.Entry;
                await RunPreCommitPhaseAsync(transition, currentPhase, context, hierarchyPlan, cancellationToken)
                    .ConfigureAwait(false);
            }

            currentPhase = TransitionLifecyclePhase.Commit;
            cancellationToken.ThrowIfCancellationRequested();
            resultingState = targetState;
            var historySnapshots = UpdateHistoryRecords(currentState, resultingState, historyRecords);
            committed = true;
            await observations.ObserveAsync(TransitionObservationKind.Committed, TransitionLifecyclePhase.Commit,
                    transition, null, true, TransitionDiagnostics.None, resultingState, cancellationToken)
                .ConfigureAwait(false);

            var activeShape = CreateCommittedActiveShape(transition, resultingState, historyResolution);
            var success = TransitionOutcome<TState, TEvent>.Success(currentState, resultingState, @event, transition,
                targetPath, historySnapshots, activeShape);
            await observations.ObserveAsync(TransitionObservationKind.Completed, TransitionLifecyclePhase.Entry,
                    transition, success, true, success.Diagnostics, resultingState, cancellationToken)
                .ConfigureAwait(false);
            await observations.OutcomeAsync(success, cancellationToken).ConfigureAwait(false);
            return success;
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken ||
                                                    cancellationToken.IsCancellationRequested)
        {
            var outcome = TransitionOutcome<TState, TEvent>.Cancelled(
                currentState,
                resultingState,
                @event,
                transition,
                new TransitionDiagnostics(
                    committed
                        ? "Transition was cancelled after the state-change point."
                        : "Transition was cancelled before the state-change point.", currentPhase, ex,
                    hierarchyMetadata: hierarchyDiagnostics ??
                                       CreateHierarchyDiagnostics(currentState, transition, resultingState)),
                committed);
            await observations.ObserveAsync(TransitionObservationKind.Cancelled, currentPhase, transition, outcome,
                committed, outcome.Diagnostics, outcome.ResultingState, cancellationToken).ConfigureAwait(false);
            await observations.OutcomeAsync(outcome, cancellationToken).ConfigureAwait(false);
            return outcome;
        }
        catch (ActionExecutionException ex)
        {
            var outcome = TransitionOutcome<TState, TEvent>.BehaviorFailure(
                currentState,
                resultingState,
                @event,
                transition,
                new TransitionDiagnostics(
                    $"{ex.Phase} action '{ex.Action.DisplayName}' failed before commit: {ex.InnerException?.Message ?? ex.Message}",
                    ex.Phase, ex.InnerException ?? ex, affectedElement: ex.Action.DisplayName,
                    hierarchyMetadata: hierarchyDiagnostics ??
                                       CreateHierarchyDiagnostics(currentState, transition, resultingState)),
                committed);
            await observations.ObserveAsync(TransitionObservationKind.BehaviorFailed, ex.Phase, transition, outcome,
                committed, outcome.Diagnostics, outcome.ResultingState, cancellationToken).ConfigureAwait(false);
            await observations.OutcomeAsync(outcome, cancellationToken).ConfigureAwait(false);
            return outcome;
        }
        catch (TransitionBehaviorException ex)
        {
            var outcome = TransitionOutcome<TState, TEvent>.BehaviorFailure(
                currentState,
                resultingState,
                @event,
                transition,
                new TransitionDiagnostics(
                    $"{ex.Phase} behavior failed before commit: {ex.InnerException?.Message ?? ex.Message}", ex.Phase,
                    ex.InnerException ?? ex,
                    hierarchyMetadata: hierarchyDiagnostics ??
                                       CreateHierarchyDiagnostics(currentState, transition, resultingState)),
                committed);
            await observations.ObserveAsync(TransitionObservationKind.BehaviorFailed, ex.Phase, transition, outcome,
                committed, outcome.Diagnostics, outcome.ResultingState, cancellationToken).ConfigureAwait(false);
            await observations.OutcomeAsync(outcome, cancellationToken).ConfigureAwait(false);
            return outcome;
        }
        catch (Exception ex)
        {
            var outcome = TransitionOutcome<TState, TEvent>.BehaviorFailure(
                currentState,
                resultingState,
                @event,
                transition,
                new TransitionDiagnostics($"Transition failed before commit: {ex.Message}", currentPhase, ex,
                    hierarchyMetadata: hierarchyDiagnostics ??
                                       CreateHierarchyDiagnostics(currentState, transition, resultingState)),
                committed);
            await observations.ObserveAsync(TransitionObservationKind.BehaviorFailed, currentPhase, transition, outcome,
                committed, outcome.Diagnostics, outcome.ResultingState, cancellationToken).ConfigureAwait(false);
            await observations.OutcomeAsync(outcome, cancellationToken).ConfigureAwait(false);
            return outcome;
        }
    }

    private async ValueTask<TransitionOutcome<TState, TEvent>> ApplyUnobservedAsync(
        TState currentState,
        TEvent @event,
        TransitionDefinition<TState, TEvent>? selectedTransition,
        CancellationToken cancellationToken,
        IDictionary<TState, CompositeHistoryRecord<TState>>? historyRecords,
        ParallelHistoryStore<TState>? parallelHistoryStore,
        ActiveStateShape<TState>? preTransitionActiveShape)
    {
        var validation = _definition.Validate();
        if (!validation.IsValid)
            return CreateValidationFailureOutcome(currentState, @event, validation);

        TransitionDefinition<TState, TEvent>? transition = null;
        HierarchySelectionDiagnostics? hierarchyDiagnostics = null;
        var currentPhase = TransitionLifecyclePhase.Matching;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            transition = selectedTransition ?? _matcher.Match(currentState, @event);
            if (transition is null)
                return TransitionOutcome<TState, TEvent>.NotPermitted(
                    currentState,
                    @event,
                    new TransitionDiagnostics($"Event '{@event}' is not permitted from state '{currentState}'.",
                        TransitionLifecyclePhase.Matching,
                        denialDiagnostics:
                        [
                            CreateNoMatchDiagnostic(currentState, @event)
                        ]));

            var targetState = ResolveTargetState(currentState, transition, historyRecords, parallelHistoryStore,
                preTransitionActiveShape, out var historyResolution);
            hierarchyDiagnostics = CreateHierarchyDiagnostics(currentState, transition, targetState, historyResolution);
            var targetPath = _definition.GetActiveStatePath(targetState);
            var hierarchyPlan = CreateHierarchyPlan(currentState, targetState, transition);
            var contextSource = transition.IsCompletionTriggered ? transition.SourceState : currentState;
            var context = new TransitionContext<TState, TEvent>(_definition, contextSource, @event, cancellationToken,
                transition, targetState, triggerKind: transition.TriggerKind);

            currentPhase = TransitionLifecyclePhase.Condition;
            var conditionResult =
                await EvaluateConditionsAsync(transition, context, cancellationToken).ConfigureAwait(false);
            if (!conditionResult.IsAllowed)
                return TransitionOutcome<TState, TEvent>.Denied(
                    currentState,
                    @event,
                    transition,
                    new TransitionDiagnostics(
                        $"Condition '{conditionResult.DeniedCondition!.DisplayName}' denied the transition.",
                        TransitionLifecyclePhase.Condition,
                        affectedElement: conditionResult.DeniedCondition.DisplayName,
                        hierarchyMetadata: hierarchyDiagnostics,
                        denialDiagnostics:
                        [
                            TransitionDenialDiagnostic.FailedGuards(
                                GetTransitionId(transition),
                                [new TransitionPreviewGuardDiagnostic(
                                    GetTransitionId(transition), 0,
                                    conditionResult.DeniedCondition.DisplayName,
                                    TransitionPreviewGuardStatus.Failed)])
                        ]));

            if (transition.Kind is TransitionKind.External or TransitionKind.Self)
            {
                currentPhase = TransitionLifecyclePhase.Exit;
                await RunPreCommitPhaseAsync(transition, currentPhase, context, hierarchyPlan, cancellationToken)
                    .ConfigureAwait(false);
            }

            currentPhase = TransitionLifecyclePhase.Transition;
            await RunPreCommitPhaseAsync(transition, currentPhase, context, hierarchyPlan, cancellationToken)
                .ConfigureAwait(false);

            if (transition.Kind is TransitionKind.External or TransitionKind.Self)
            {
                currentPhase = TransitionLifecyclePhase.Entry;
                await RunPreCommitPhaseAsync(transition, currentPhase, context, hierarchyPlan, cancellationToken)
                    .ConfigureAwait(false);
            }

            currentPhase = TransitionLifecyclePhase.Commit;
            cancellationToken.ThrowIfCancellationRequested();
            var committedState = targetState;
            var historySnapshots = UpdateHistoryRecords(currentState, committedState, historyRecords);

            var activeShape = CreateCommittedActiveShape(transition, committedState, historyResolution);
            return TransitionOutcome<TState, TEvent>.Success(currentState, committedState, @event, transition,
                targetPath, historySnapshots, activeShape);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken ||
                                                    cancellationToken.IsCancellationRequested)
        {
            return TransitionOutcome<TState, TEvent>.Cancelled(
                currentState,
                currentState,
                @event,
                transition,
                new TransitionDiagnostics("Transition was cancelled before the state-change point.", currentPhase, ex,
                    hierarchyMetadata: hierarchyDiagnostics ??
                                       CreateHierarchyDiagnostics(currentState, transition, currentState)),
                false);
        }
        catch (ActionExecutionException ex)
        {
            return TransitionOutcome<TState, TEvent>.BehaviorFailure(
                currentState,
                currentState,
                @event,
                transition,
                new TransitionDiagnostics(
                    $"{ex.Phase} action '{ex.Action.DisplayName}' failed before commit: {ex.InnerException?.Message ?? ex.Message}",
                    ex.Phase, ex.InnerException ?? ex, affectedElement: ex.Action.DisplayName,
                    hierarchyMetadata: hierarchyDiagnostics ??
                                       CreateHierarchyDiagnostics(currentState, transition, currentState)),
                false);
        }
        catch (TransitionBehaviorException ex)
        {
            return TransitionOutcome<TState, TEvent>.BehaviorFailure(
                currentState,
                currentState,
                @event,
                transition,
                new TransitionDiagnostics(
                    $"{ex.Phase} behavior failed before commit: {ex.InnerException?.Message ?? ex.Message}", ex.Phase,
                    ex.InnerException ?? ex,
                    hierarchyMetadata: hierarchyDiagnostics ??
                                       CreateHierarchyDiagnostics(currentState, transition, currentState)),
                false);
        }
        catch (Exception ex)
        {
            return TransitionOutcome<TState, TEvent>.BehaviorFailure(
                currentState,
                currentState,
                @event,
                transition,
                new TransitionDiagnostics($"Transition failed before commit: {ex.Message}", currentPhase, ex,
                    hierarchyMetadata: hierarchyDiagnostics ??
                                       CreateHierarchyDiagnostics(currentState, transition, currentState)),
                false);
        }
    }

    private static TransitionOutcome<TState, TEvent> CreateValidationFailureOutcome(
        TState currentState,
        TEvent @event,
        ValidationResult validation)
    {
        return TransitionOutcome<TState, TEvent>.ValidationFailure(
            currentState,
            @event,
            new TransitionDiagnostics("Machine definition has validation errors.",
                validationFindings: validation.Errors,
                conflictDiagnostics: validation.ConflictDiagnostics,
                denialDiagnostics:
                [
                    TransitionDenialDiagnostic.ValidationConflicts(validation.Errors,
                        validation.ConflictDiagnostics)
                ]));
    }

    private async ValueTask<ConditionEvaluationResult<TState, TEvent>> EvaluateConditionsAsync(
        TransitionDefinition<TState, TEvent> transition,
        TransitionContext<TState, TEvent> context,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _conditionEvaluator.EvaluateAsync(transition, context, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new TransitionBehaviorException(TransitionLifecyclePhase.Condition, ex);
        }
    }

    private TransitionDenialDiagnostic CreateNoMatchDiagnostic(TState currentState, TEvent @event)
    {
        var eventIdentity = ResolveEventIdentity(@event);
        if (_definition.FindState(currentState)?.IsTerminal == true)
            return TransitionDenialDiagnostic.TerminalState(currentState, eventIdentity);

        return eventIdentity is null
            ? TransitionDenialDiagnostic.UnknownEvent(Convert.ToString(@event,
                System.Globalization.CultureInfo.InvariantCulture))
            : TransitionDenialDiagnostic.NoMatchingEvent(currentState, eventIdentity);
    }

    private string? ResolveEventIdentity(TEvent @event)
    {
        foreach (var eventDefinition in _definition.Events)
            if (eventDefinition.Matches(@event))
                return eventDefinition.Identity;

        return null;
    }

    private string GetTransitionId(TransitionDefinition<TState, TEvent> transition)
    {
        return TransitionIdentityProvider.GetTransitionId(_definition, transition) ?? transition.ToString();
    }

    private TState ResolveTargetState(
        TState currentState,
        TransitionDefinition<TState, TEvent> transition,
        IDictionary<TState, CompositeHistoryRecord<TState>>? historyRecords,
        ParallelHistoryStore<TState>? parallelHistoryStore,
        ActiveStateShape<TState>? preTransitionActiveShape,
        out HistoryResolution<TState>? historyResolution)
    {
        historyResolution = null;
        if (transition.Kind == TransitionKind.Internal) return currentState;

        if (_definition.HasHistoryFor(transition.TargetState))
        {
            historyResolution = HistoryTargetResolver.Resolve(_definition, transition.TargetState, historyRecords,
                parallelHistoryStore, preTransitionActiveShape);
            return historyResolution.ResolvedTargetLeafState;
        }

        return InitialChildResolver.ResolveTargetLeaf(_definition, transition.TargetState);
    }

    private ActiveStateShape<TState> CreateCommittedActiveShape(TransitionDefinition<TState, TEvent> transition,
        TState resultingState, HistoryResolution<TState>? historyResolution)
    {
        if (!_definition.IsParallelComposite(transition.TargetState))
            return ActiveStateShape<TState>.Single(resultingState);

        if (historyResolution?.ParallelRestorePlan?.IsValid == true)
            return historyResolution.ParallelRestorePlan.PlannedPostRestoreShape;

        return ParallelRegionInitialResolver.Enter(_definition, transition.TargetState);
    }

    private HierarchySelectionDiagnostics? CreateHierarchyDiagnostics(TState activeLeafState,
        TransitionDefinition<TState, TEvent>? transition, TState resolvedTargetLeafState,
        HistoryResolution<TState>? historyResolution = null)
    {
        if (!_definition.HasHierarchy || transition is null) return null;

        var comparer = EqualityComparer<TState>.Default;
        var sourcePath = _definition.GetActiveStatePath(activeLeafState);
        var targetPath = _definition.GetActiveStatePath(resolvedTargetLeafState);

        return new HierarchySelectionDiagnostics(
            activeLeafState,
            transition.SourceState,
            transition.TargetState,
            resolvedTargetLeafState,
            sourcePath.Depth,
            targetPath.Depth,
            !comparer.Equals(activeLeafState, transition.SourceState),
            !comparer.Equals(transition.TargetState, resolvedTargetLeafState),
            historyResolution?.RestoreKind.ToString());
    }

    private HierarchyEntryExitPlan<TState>? CreateHierarchyPlan(TState currentState, TState targetState,
        TransitionDefinition<TState, TEvent> transition)
    {
        if (!_definition.HasHierarchy ||
            transition.Kind is not (TransitionKind.External or TransitionKind.Self)) return null;

        return HierarchyEntryExitPlanner.Plan(_definition.GetActiveStatePath(currentState),
            _definition.GetActiveStatePath(targetState));
    }


    private IReadOnlyList<CompositeHistorySnapshot<TState>> UpdateHistoryRecords(
        TState previousState,
        TState resultingState,
        IDictionary<TState, CompositeHistoryRecord<TState>>? historyRecords)
    {
        if (!_definition.HasHistory || historyRecords is null) return Array.Empty<CompositeHistorySnapshot<TState>>();

        var nextSequence = historyRecords.Count == 0 ? 1 : historyRecords.Values.Max(r => r.LastUpdatedSequence) + 1;
        UpdateHistoryForPath(previousState, historyRecords, ref nextSequence);
        UpdateHistoryForPath(resultingState, historyRecords, ref nextSequence);
        return CreateHistorySnapshots(historyRecords);
    }

    private void UpdateHistoryForPath(TState leafState,
        IDictionary<TState, CompositeHistoryRecord<TState>> historyRecords, ref long sequence)
    {
        var path = _definition.GetActiveStatePath(leafState).StatesRootToLeaf;
        if (path.Count < 2) return;

        for (var i = 0; i < path.Count - 1; i++)
        {
            var composite = path[i];
            if (!_definition.HasHistoryFor(composite)) continue;

            historyRecords[composite] = new CompositeHistoryRecord<TState>(
                composite,
                path[i + 1],
                leafState,
                sequence++);
        }
    }

    private IReadOnlyList<CompositeHistorySnapshot<TState>> CreateHistorySnapshots(
        IDictionary<TState, CompositeHistoryRecord<TState>> historyRecords)
    {
        return _definition.HistoryEnabledStates
            .Select(state =>
            {
                historyRecords.TryGetValue(state.Value, out var record);
                _definition.TryGetEffectiveHistoryFallback(state.Value, out var fallback);
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

    private async ValueTask RunPreCommitPhaseAsync(
        TransitionDefinition<TState, TEvent> transition,
        TransitionLifecyclePhase phase,
        TransitionContext<TState, TEvent> context,
        HierarchyEntryExitPlan<TState>? hierarchyPlan,
        CancellationToken cancellationToken)
    {
        try
        {
            var targetState = context.TargetState ?? context.SourceState;
            var actionContext = new ActionExecutionContext<TState, TEvent>(_definition, context.SourceState,
                targetState, context.Event, transition, phase, cancellationToken, context.Metadata,
                triggerKind: context.TriggerKind);

            if (phase == TransitionLifecyclePhase.Exit)
            {
                var exitStates = hierarchyPlan?.ExitStatesLeafToBoundary ?? [transition.SourceState];
                foreach (var state in exitStates)
                    await _actionRunner.RunStateActionsAsync(_definition.FindState(state), ActionKind.Exit, transition,
                        actionContext, cancellationToken).ConfigureAwait(false);
            }

            if (phase == TransitionLifecyclePhase.Transition)
                await _actionRunner.RunTransitionActionsAsync(transition, actionContext, cancellationToken)
                    .ConfigureAwait(false);

            await _behaviorRunner.RunPhaseAsync(transition, phase, context, cancellationToken).ConfigureAwait(false);

            if (phase == TransitionLifecyclePhase.Entry)
            {
                var entryStates = hierarchyPlan?.EntryStatesBoundaryToLeaf ?? [targetState];
                foreach (var state in entryStates)
                    await _actionRunner.RunStateActionsAsync(_definition.FindState(state), ActionKind.Entry, transition,
                        actionContext, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new TransitionBehaviorException(phase, ex);
        }
    }

    private sealed class TransitionBehaviorException(TransitionLifecyclePhase phase, Exception innerException)
        : Exception(innerException.Message, innerException)
    {
        public TransitionLifecyclePhase Phase { get; } = phase;
    }
}