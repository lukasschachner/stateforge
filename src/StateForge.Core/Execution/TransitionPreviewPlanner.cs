using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;
using StateForge.Core.Validation;

namespace StateForge.Core.Execution;

internal sealed class TransitionPreviewPlanner<TState, TEvent>
{
    private readonly StateMachineDefinition<TState, TEvent> _definition;
    private readonly ConditionEvaluator<TState, TEvent> _conditionEvaluator;

    public TransitionPreviewPlanner(StateMachineDefinition<TState, TEvent> definition)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _conditionEvaluator = new ConditionEvaluator<TState, TEvent>(_definition);
    }

    public async ValueTask<TransitionPreviewResult<TState, TEvent>> PreviewAsync(
        ActiveStateShape<TState> activeShape,
        TEvent @event,
        CancellationToken cancellationToken = default,
        IDictionary<TState, CompositeHistoryRecord<TState>>? historyRecords = null,
        ParallelHistoryStore<TState>? parallelHistoryStore = null)
    {
        ArgumentNullException.ThrowIfNull(activeShape);

        var validation = _definition.Validate();
        if (!validation.IsValid)
            return new TransitionPreviewResult<TState, TEvent>(
                TransitionPreviewStatus.ValidationFailure,
                @event,
                activeShape,
                denialDiagnostic: TransitionDenialDiagnostic.ValidationConflicts(validation.Errors,
                    validation.ConflictDiagnostics),
                validationFindings: validation.Errors,
                conflictDiagnostics: validation.ConflictDiagnostics);

        var shapeValidation = ValidateActiveShape(activeShape);
        if (!shapeValidation.IsValid)
            return new TransitionPreviewResult<TState, TEvent>(
                TransitionPreviewStatus.InvalidActiveShape,
                @event,
                activeShape,
                denialDiagnostic: TransitionDenialDiagnostic.InvalidActiveShape(shapeValidation.Diagnostics));

        if (cancellationToken.IsCancellationRequested)
            return new TransitionPreviewResult<TState, TEvent>(
                TransitionPreviewStatus.Cancelled,
                @event,
                activeShape,
                denialDiagnostic: TransitionDenialDiagnostic.GuardCancelled(null,
                    Array.Empty<TransitionPreviewGuardDiagnostic>()));

        var eventIdentity = ResolveEventIdentity(@event);
        if (eventIdentity is null)
            return new TransitionPreviewResult<TState, TEvent>(
                TransitionPreviewStatus.Denied,
                @event,
                activeShape,
                denialDiagnostic: TransitionDenialDiagnostic.UnknownEvent(Convert.ToString(@event,
                    System.Globalization.CultureInfo.InvariantCulture)));

        return activeShape.IsParallel
            ? await PreviewParallelAsync(activeShape, @event, eventIdentity, cancellationToken, historyRecords,
                    parallelHistoryStore)
                .ConfigureAwait(false)
            : await PreviewSingleAsync(activeShape, @event, eventIdentity, cancellationToken, historyRecords,
                    parallelHistoryStore)
                .ConfigureAwait(false);
    }

    private async ValueTask<TransitionPreviewResult<TState, TEvent>> PreviewSingleAsync(
        ActiveStateShape<TState> activeShape,
        TEvent @event,
        string eventIdentity,
        CancellationToken cancellationToken,
        IDictionary<TState, CompositeHistoryRecord<TState>>? historyRecords,
        ParallelHistoryStore<TState>? parallelHistoryStore)
    {
        var current = activeShape.ActiveLeafState!;
        if (!_definition.ContainsState(current))
            return new TransitionPreviewResult<TState, TEvent>(
                TransitionPreviewStatus.InvalidActiveShape,
                @event,
                activeShape,
                denialDiagnostic: TransitionDenialDiagnostic.UnknownCurrentState(current));

        var matcher = new TransitionMatcher<TState, TEvent>(_definition);
        var transition = matcher.Match(current, @event);
        var candidates = EnumerateCandidates(current, @event, null, null, transition);
        if (transition is null)
        {
            var terminal = _definition.FindState(current)?.IsTerminal == true;
            return new TransitionPreviewResult<TState, TEvent>(
                TransitionPreviewStatus.Denied,
                @event,
                activeShape,
                candidateTransitions: candidates,
                denialDiagnostic: terminal
                    ? TransitionDenialDiagnostic.TerminalState(current, eventIdentity)
                    : TransitionDenialDiagnostic.NoMatchingEvent(current, eventIdentity));
        }

        var target = ResolveTargetState(current, transition, historyRecords, parallelHistoryStore, activeShape,
            out var historyResolution);
        var guardResult = await EvaluateGuardsAsync(transition, current, target, @event, cancellationToken)
            .ConfigureAwait(false);
        var candidateWithGuards = new[] { CreateCandidate(transition, "Selected", null, null, guardResult.Diagnostics) };
        if (guardResult.Status == GuardPreviewStatus.Cancelled)
            return new TransitionPreviewResult<TState, TEvent>(
                TransitionPreviewStatus.Cancelled,
                @event,
                activeShape,
                transition,
                candidateTransitions: candidateWithGuards,
                guardDiagnostics: guardResult.Diagnostics,
                denialDiagnostic: TransitionDenialDiagnostic.GuardCancelled(GetTransitionId(transition),
                    guardResult.Diagnostics));

        if (guardResult.Status == GuardPreviewStatus.Error)
            return new TransitionPreviewResult<TState, TEvent>(
                TransitionPreviewStatus.GuardEvaluationFailed,
                @event,
                activeShape,
                transition,
                candidateTransitions: candidateWithGuards,
                guardDiagnostics: guardResult.Diagnostics,
                denialDiagnostic: TransitionDenialDiagnostic.GuardFailed(GetTransitionId(transition),
                    guardResult.Diagnostics));

        if (!guardResult.IsAllowed)
            return new TransitionPreviewResult<TState, TEvent>(
                TransitionPreviewStatus.Denied,
                @event,
                activeShape,
                transition,
                candidateTransitions: candidateWithGuards,
                guardDiagnostics: guardResult.Diagnostics,
                denialDiagnostic: TransitionDenialDiagnostic.FailedGuards(GetTransitionId(transition),
                    guardResult.Diagnostics));

        var expectedShape = CreateExpectedShape(transition, target, historyResolution, activeShape,
            out var completeness);
        return new TransitionPreviewResult<TState, TEvent>(
            TransitionPreviewStatus.Permitted,
            @event,
            activeShape,
            transition,
            candidateTransitions: candidateWithGuards,
            guardDiagnostics: guardResult.Diagnostics,
            expectedTargetState: target,
            expectedActiveShape: expectedShape,
            predictionCompleteness: completeness);
    }

    private async ValueTask<TransitionPreviewResult<TState, TEvent>> PreviewParallelAsync(
        ActiveStateShape<TState> activeShape,
        TEvent @event,
        string eventIdentity,
        CancellationToken cancellationToken,
        IDictionary<TState, CompositeHistoryRecord<TState>>? historyRecords,
        ParallelHistoryStore<TState>? parallelHistoryStore)
    {
        var owner = activeShape.OwningCompositeState!;
        var parentTransition = new TransitionMatcher<TState, TEvent>(_definition).Match(owner, @event);
        var matches = new ParallelTransitionResolver<TState, TEvent>(_definition).Resolve(activeShape, @event);

        if (matches.Count == 0)
        {
            if (parentTransition is not null)
                return await PreviewSingleAsync(ActiveStateShape<TState>.Single(owner, activeShape.Sequence), @event,
                        eventIdentity, cancellationToken, historyRecords, parallelHistoryStore)
                    .ConfigureAwait(false);

            var allTerminal = activeShape.ActiveRegions.Count > 0 && activeShape.ActiveRegions.All(r => r.IsTerminal);
            return new TransitionPreviewResult<TState, TEvent>(
                TransitionPreviewStatus.Denied,
                @event,
                activeShape,
                denialDiagnostic: allTerminal
                    ? TransitionDenialDiagnostic.TerminalState(owner, eventIdentity)
                    : TransitionDenialDiagnostic.NoMatchingEvent(owner, eventIdentity));
        }

        var guardDiagnostics = new List<TransitionPreviewGuardDiagnostic>();
        var candidates = new List<TransitionPreviewCandidate>();
        foreach (var (region, transition) in matches)
        {
            var target = transition.Kind == TransitionKind.Internal
                ? region.ActiveLeafState
                : InitialChildResolver.ResolveTargetLeaf(_definition, transition.TargetState);
            var guardResult = await EvaluateGuardsAsync(transition, region.ActiveLeafState, target, @event,
                    cancellationToken)
                .ConfigureAwait(false);
            guardDiagnostics.AddRange(guardResult.Diagnostics);
            candidates.Add(CreateCandidate(transition, guardResult.IsAllowed ? "RegionalParticipant" : "DeniedByGuard",
                region.RegionId, region.RegionName, guardResult.Diagnostics));

            if (guardResult.Status == GuardPreviewStatus.Cancelled)
                return new TransitionPreviewResult<TState, TEvent>(
                    TransitionPreviewStatus.Cancelled,
                    @event,
                    activeShape,
                    transition,
                    matches.Select(m => m.Transition).ToArray(),
                    candidates,
                    guardDiagnostics,
                    denialDiagnostic: TransitionDenialDiagnostic.GuardCancelled(GetTransitionId(transition),
                        guardResult.Diagnostics));

            if (guardResult.Status == GuardPreviewStatus.Error)
                return new TransitionPreviewResult<TState, TEvent>(
                    TransitionPreviewStatus.GuardEvaluationFailed,
                    @event,
                    activeShape,
                    transition,
                    matches.Select(m => m.Transition).ToArray(),
                    candidates,
                    guardDiagnostics,
                    denialDiagnostic: TransitionDenialDiagnostic.GuardFailed(GetTransitionId(transition),
                        guardResult.Diagnostics));

            if (!guardResult.IsAllowed)
                return new TransitionPreviewResult<TState, TEvent>(
                    TransitionPreviewStatus.Denied,
                    @event,
                    activeShape,
                    transition,
                    matches.Select(m => m.Transition).ToArray(),
                    candidates,
                    guardDiagnostics,
                    denialDiagnostic: TransitionDenialDiagnostic.FailedGuards(GetTransitionId(transition),
                        guardResult.Diagnostics));
        }

        var entries = activeShape.ActiveRegions.ToDictionary(e => e.RegionId, StringComparer.Ordinal);
        foreach (var (region, transition) in matches)
        {
            var target = transition.Kind == TransitionKind.Internal
                ? region.ActiveLeafState
                : InitialChildResolver.ResolveTargetLeaf(_definition, transition.TargetState);
            var regionDefinition = _definition.GetParallelRegions(owner).First(r => r.RegionId == region.RegionId);
            entries[region.RegionId] = new ActiveRegionEntry<TState>(region.RegionId, region.RegionName, target,
                _definition.GetActiveStatePath(target),
                regionDefinition.TerminalStates.Contains(target, EqualityComparer<TState>.Default));
        }

        var ordered = _definition.GetParallelRegions(owner).Select(r => entries[r.RegionId]).ToArray();
        var expectedShape = ActiveStateShape<TState>.Parallel(owner, ordered, activeShape.Sequence + 1);
        var transitions = matches.Select(m => m.Transition).Distinct().ToArray();
        var parentIsCompletion = parentTransition is not null &&
                                 ParallelCompletionEvaluator.IsComplete(_definition, expectedShape);
        var conflicts = ParallelConflictDetector.Detect(_definition, transitions, parentTransition, parentIsCompletion, @event);
        if (conflicts.Count > 0)
        {
            var conflictDiagnostics = conflicts.SelectMany(conflict => conflict.ConflictDiagnostics).ToArray();
            return new TransitionPreviewResult<TState, TEvent>(
                TransitionPreviewStatus.Denied,
                @event,
                activeShape,
                transitions[0],
                transitions,
                candidates,
                guardDiagnostics,
                denialDiagnostic: new TransitionDenialDiagnostic(
                    TransitionDenialReason.AmbiguousTransitions,
                    conflicts[0].Summary,
                    TransitionLifecyclePhase.Matching,
                    eventIdentity: eventIdentity,
                    candidateTransitionIds: transitions.Select(GetTransitionId).ToArray(),
                    conflictDiagnostics: conflictDiagnostics),
                conflictDiagnostics: conflictDiagnostics);
        }

        return new TransitionPreviewResult<TState, TEvent>(
            TransitionPreviewStatus.Permitted,
            @event,
            activeShape,
            transitions[0],
            transitions,
            candidates,
            guardDiagnostics,
            expectedTargetState: ordered[0].ActiveLeafState,
            expectedActiveShape: expectedShape,
            predictionCompleteness: TransitionPredictionCompleteness.Complete);
    }

    private ActiveStateSnapshotValidationResult<TState> ValidateActiveShape(ActiveStateShape<TState> activeShape)
    {
        var snapshot = activeShape.ToActiveStateSnapshot(_definition);
        return ActiveStateSnapshotValidator.Validate(_definition, snapshot);
    }

    private string? ResolveEventIdentity(TEvent @event)
    {
        return RuntimeTransitionHelpers.ResolveEventIdentity(_definition, @event);
    }

    private IReadOnlyList<TransitionPreviewCandidate> EnumerateCandidates(
        TState current,
        TEvent @event,
        string? regionId,
        string? regionName,
        TransitionDefinition<TState, TEvent>? selected)
    {
        return _definition.Transitions
            .Where(t => t.Event.Matches(@event))
            .Where(t => EqualityComparer<TState>.Default.Equals(t.SourceState, current) ||
                        (_definition.HasHierarchy && _definition.GetActiveStatePath(current).StatesRootToLeaf
                            .Contains(t.SourceState, EqualityComparer<TState>.Default)))
            .Select(t => CreateCandidate(t, ReferenceEquals(t, selected) ? "Selected" : "Skipped", regionId, regionName))
            .ToArray();
    }

    private async ValueTask<GuardPreviewResult> EvaluateGuardsAsync(
        TransitionDefinition<TState, TEvent> transition,
        TState current,
        TState target,
        TEvent @event,
        CancellationToken cancellationToken)
    {
        var context = new TransitionContext<TState, TEvent>(_definition, current, @event, cancellationToken, transition,
            target, triggerKind: transition.TriggerKind);
        try
        {
            return await _conditionEvaluator.EvaluateWithDiagnosticsAsync(transition, context, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return GuardPreviewResult.Cancelled([new TransitionPreviewGuardDiagnostic(GetTransitionId(transition), 0,
                "Cancellation", TransitionPreviewGuardStatus.Cancelled, "Guard evaluation was cancelled.")]);
        }
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

    private ActiveStateShape<TState>? CreateExpectedShape(
        TransitionDefinition<TState, TEvent> transition,
        TState target,
        HistoryResolution<TState>? historyResolution,
        ActiveStateShape<TState> activeShape,
        out TransitionPredictionCompleteness completeness)
    {
        completeness = TransitionPredictionCompleteness.Complete;
        if (transition.Kind == TransitionKind.Internal) return activeShape;

        if (!_definition.IsParallelComposite(transition.TargetState))
            return ActiveStateShape<TState>.Single(target, activeShape.Sequence + 1);

        if (historyResolution?.ParallelRestorePlan?.IsValid == true)
            return historyResolution.ParallelRestorePlan.PlannedPostRestoreShape;

        return ParallelRegionInitialResolver.Enter(_definition, transition.TargetState, activeShape.Sequence + 1);
    }

    private TransitionPreviewCandidate CreateCandidate(
        TransitionDefinition<TState, TEvent> transition,
        string role,
        string? regionId,
        string? regionName,
        IReadOnlyList<TransitionPreviewGuardDiagnostic>? guardDiagnostics = null)
    {
        return new TransitionPreviewCandidate(GetTransitionId(transition), transition.SourceState, transition.Event.Identity,
            transition.TargetState, transition.TriggerKind, role, regionId, regionName, guardDiagnostics);
    }

    private string GetTransitionId(TransitionDefinition<TState, TEvent> transition)
    {
        return TransitionIdentityProvider.GetTransitionId(_definition, transition) ?? transition.ToString();
    }
}

internal enum GuardPreviewStatus
{
    Allowed,
    Denied,
    Cancelled,
    Error
}

internal sealed class GuardPreviewResult
{
    private GuardPreviewResult(GuardPreviewStatus status, IReadOnlyList<TransitionPreviewGuardDiagnostic> diagnostics)
    {
        Status = status;
        Diagnostics = diagnostics;
    }

    public GuardPreviewStatus Status { get; }
    public IReadOnlyList<TransitionPreviewGuardDiagnostic> Diagnostics { get; }
    public bool IsAllowed => Status == GuardPreviewStatus.Allowed;

    public static GuardPreviewResult Allowed(IReadOnlyList<TransitionPreviewGuardDiagnostic> diagnostics) =>
        new(GuardPreviewStatus.Allowed, diagnostics);

    public static GuardPreviewResult Denied(IReadOnlyList<TransitionPreviewGuardDiagnostic> diagnostics) =>
        new(GuardPreviewStatus.Denied, diagnostics);

    public static GuardPreviewResult Cancelled(IReadOnlyList<TransitionPreviewGuardDiagnostic> diagnostics) =>
        new(GuardPreviewStatus.Cancelled, diagnostics);

    public static GuardPreviewResult Error(IReadOnlyList<TransitionPreviewGuardDiagnostic> diagnostics) =>
        new(GuardPreviewStatus.Error, diagnostics);
}
