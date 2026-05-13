using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Core.Introspection;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Definitions;

/// <summary>Immutable reusable finite state machine definition.</summary>
public sealed class StateMachineDefinition<TState, TEvent>
{
    private readonly Dictionary<StateKey<TState>, IReadOnlyList<StateDefinition<TState>>> _childrenByParent;
    private readonly Dictionary<StateKey<TState>, StateDefinition<TState>> _historyByComposite;
    private readonly Dictionary<StateKey<TState>, StateDefinition<TState>> _initialChildByParent;
    private readonly Dictionary<StateKey<TState>, StateDefinition<TState>> _parentByChild;
    private readonly Dictionary<StateKey<TState>, StateRegionMembershipDefinition<TState>> _regionMembershipByState;
    private readonly Dictionary<StateKey<TState>, IReadOnlyList<ParallelRegionDefinition<TState>>> _regionsByComposite;
    private readonly Dictionary<string, ParallelRegionDefinition<TState>> _regionsById;
    private readonly Dictionary<StateKey<TState>, StateDefinition<TState>> _stateLookup;
    private readonly Lazy<ValidationResult> _validation;

    private StateMachineDefinition(
        IEnumerable<StateDefinition<TState>> states,
        IEnumerable<TransitionDefinition<TState, TEvent>> transitions,
        MetadataCollection? metadata = null,
        IEnumerable<ParallelRegionDefinition<TState>>? parallelRegions = null,
        IEnumerable<CompletionTransitionDefinition<TState, TEvent>>? completionTransitions = null)
    {
        States = states.ToArray();
        Transitions = transitions.ToArray();
        CompletionTransitions = (completionTransitions ?? []).OrderBy(t => t.DeclarationOrder).ToArray();
        ParallelRegions = (parallelRegions ?? []).OrderBy(r => r.Order).ToArray();
        Events = Transitions.Select(t => t.Event).GroupBy(e => e.Identity).Select(g => g.First()).ToArray();
        TerminalStates = States.Where(s => s.IsTerminal).ToArray();
        Metadata = metadata ?? MetadataCollection.Empty;
        _stateLookup = States.GroupBy(s => new StateKey<TState>(s.Value)).ToDictionary(g => g.Key, g => g.First());
        _parentByChild = States
            .Where(s => s.Hierarchy.HasParent)
            .Where(s => ContainsState(s.Hierarchy.ParentState!))
            .ToDictionary(s => new StateKey<TState>(s.Value), s => FindState(s.Hierarchy.ParentState!)!);
        _childrenByParent = States
            .Where(s => s.Hierarchy.HasParent)
            .Where(s => ContainsState(s.Hierarchy.ParentState!))
            .GroupBy(s => new StateKey<TState>(s.Hierarchy.ParentState!))
            .ToDictionary(g => g.Key, g => (IReadOnlyList<StateDefinition<TState>>)g.ToArray());
        _initialChildByParent = States
            .Where(s => s.Hierarchy.HasInitialChild)
            .Where(s => ContainsState(s.Hierarchy.InitialChildState!))
            .ToDictionary(s => new StateKey<TState>(s.Value), s => FindState(s.Hierarchy.InitialChildState!)!);
        _historyByComposite = States
            .Where(s => s.Hierarchy.HasHistory)
            .ToDictionary(s => new StateKey<TState>(s.Value), s => s);
        _regionsByComposite = ParallelRegions
            .GroupBy(r => new StateKey<TState>(r.OwnerCompositeState))
            .ToDictionary(g => g.Key,
                g => (IReadOnlyList<ParallelRegionDefinition<TState>>)g.OrderBy(r => r.Order).ToArray());
        _regionsById = ParallelRegions.ToDictionary(r => r.RegionId, StringComparer.Ordinal);
        _regionMembershipByState = BuildRegionMembershipLookup();
        HasHierarchy = States.Any(s => s.Hierarchy.HasHierarchyMetadata) || ParallelRegions.Count > 0;
        HasHistory = _historyByComposite.Count > 0;
        HasParallelRegions = ParallelRegions.Count > 0;
        HistoryEnabledStates = _historyByComposite.Values.ToArray();
        _validation = new Lazy<ValidationResult>(() => MachineDefinitionValidator.Validate(this));
    }

    /// <summary>Declared states in builder insertion order.</summary>
    public IReadOnlyList<StateDefinition<TState>> States { get; }

    /// <summary>Declared event descriptors inferred from transitions.</summary>
    public IReadOnlyList<EventDefinition<TEvent>> Events { get; }

    /// <summary>Declared transition rules.</summary>
    public IReadOnlyList<TransitionDefinition<TState, TEvent>> Transitions { get; }

    /// <summary>Declared completion transition rules.</summary>
    public IReadOnlyList<CompletionTransitionDefinition<TState, TEvent>> CompletionTransitions { get; }

    /// <summary>States marked as terminal.</summary>
    public IReadOnlyList<StateDefinition<TState>> TerminalStates { get; }

    /// <summary>Declared parallel regions in deterministic declaration order.</summary>
    public IReadOnlyList<ParallelRegionDefinition<TState>> ParallelRegions { get; }

    /// <summary>Definition-level extension metadata.</summary>
    public MetadataCollection Metadata { get; }

    /// <summary>Gets a value indicating whether any hierarchy metadata is declared.</summary>
    public bool HasHierarchy { get; }

    /// <summary>Gets a value indicating whether any composite has history enabled.</summary>
    public bool HasHistory { get; }

    /// <summary>Gets a value indicating whether any state declares parallel regions.</summary>
    public bool HasParallelRegions { get; }

    /// <summary>States that declare history metadata.</summary>
    public IReadOnlyList<StateDefinition<TState>> HistoryEnabledStates { get; }

    public static StateMachineDefinition<TState, TEvent> Create(
        Action<StateMachineDefinitionBuilder<TState, TEvent>> configure)
    {
        var builder = new StateMachineDefinitionBuilder<TState, TEvent>();
        configure(builder);
        return new StateMachineDefinition<TState, TEvent>(builder.States, builder.Transitions, builder.Metadata,
            builder.ParallelRegions, builder.CompletionTransitions);
    }

    public ValidationResult Validate()
    {
        return _validation.Value;
    }

    /// <summary>Validates externally supplied parallel-history snapshot data against this immutable definition.</summary>
    public ValidationResult ValidateParallelHistorySnapshot(ParallelHistorySnapshot<TState> snapshot)
    {
        return ParallelRegionHistoryValidator.ValidateSuppliedSnapshot(this, snapshot);
    }

    public bool ContainsState(TState state)
    {
        return _stateLookup.ContainsKey(new StateKey<TState>(state));
    }

    public StateDefinition<TState>? FindState(TState state)
    {
        return _stateLookup.GetValueOrDefault(new StateKey<TState>(state));
    }

    public bool IsCompositeState(TState state)
    {
        return _childrenByParent.ContainsKey(new StateKey<TState>(state)) || IsParallelComposite(state);
    }

    public bool IsLeafState(TState state)
    {
        return ContainsState(state) && !IsCompositeState(state);
    }

    public bool IsParallelComposite(TState state)
    {
        return _regionsByComposite.ContainsKey(new StateKey<TState>(state)) ||
               FindState(state)?.Hierarchy.IsParallelComposite == true;
    }

    public IReadOnlyList<ParallelRegionDefinition<TState>> GetParallelRegions(TState compositeState)
    {
        return _regionsByComposite.GetValueOrDefault(new StateKey<TState>(compositeState)) ??
               Array.Empty<ParallelRegionDefinition<TState>>();
    }

    public bool TryGetParallelRegion(string regionId, out ParallelRegionDefinition<TState> region)
    {
        return _regionsById.TryGetValue(regionId, out region!);
    }

    public bool TryGetRegionMembership(TState state, out StateRegionMembershipDefinition<TState> membership)
    {
        return _regionMembershipByState.TryGetValue(new StateKey<TState>(state), out membership!);
    }

    public bool TryGetCommonParallelOwner(TState source, TState target, out TState owner, out string? sourceRegionId,
        out string? targetRegionId)
    {
        sourceRegionId = null;
        targetRegionId = null;
        if (TryGetRegionMembership(source, out var sourceMembership) &&
            TryGetRegionMembership(target, out var targetMembership) &&
            EqualityComparer<TState>.Default.Equals(sourceMembership.OwnerCompositeState,
                targetMembership.OwnerCompositeState))
        {
            owner = sourceMembership.OwnerCompositeState;
            sourceRegionId = sourceMembership.RegionId;
            targetRegionId = targetMembership.RegionId;
            return true;
        }

        owner = default!;
        return false;
    }

    public IReadOnlyList<StateDefinition<TState>> GetChildren(TState parentState)
    {
        return _childrenByParent.GetValueOrDefault(new StateKey<TState>(parentState)) ??
               Array.Empty<StateDefinition<TState>>();
    }

    public bool TryGetParent(TState childState, out TState parentState)
    {
        if (_parentByChild.TryGetValue(new StateKey<TState>(childState), out var parent))
        {
            parentState = parent.Value;
            return true;
        }

        parentState = default!;
        return false;
    }

    public bool TryGetInitialChild(TState compositeState, out TState initialChildState)
    {
        if (_initialChildByParent.TryGetValue(new StateKey<TState>(compositeState), out var child))
        {
            initialChildState = child.Value;
            return true;
        }

        initialChildState = default!;
        return false;
    }

    public bool HasHistoryFor(TState compositeState)
    {
        return _historyByComposite.ContainsKey(new StateKey<TState>(compositeState));
    }

    public bool TryGetHistoryDefinition(TState compositeState, out StateDefinition<TState> state)
    {
        return _historyByComposite.TryGetValue(new StateKey<TState>(compositeState), out state!);
    }

    public bool TryGetEffectiveHistoryFallback(TState compositeState, out TState fallbackChildState)
    {
        if (TryGetHistoryDefinition(compositeState, out var state) && state.Hierarchy.HasHistoryFallback)
        {
            fallbackChildState = state.Hierarchy.HistoryFallbackState!;
            return ContainsState(fallbackChildState);
        }

        return TryGetInitialChild(compositeState, out fallbackChildState!);
    }

    public ActiveStatePath<TState> GetActiveStatePath(TState activeLeafState)
    {
        var path = new List<TState>();
        var seen = new HashSet<StateKey<TState>>();
        var current = activeLeafState;
        while (ContainsState(current) && seen.Add(new StateKey<TState>(current)))
        {
            path.Add(current);
            if (!TryGetParent(current, out var parent)) break;

            current = parent;
        }

        path.Reverse();
        return new ActiveStatePath<TState>(path.Count == 0 ? [activeLeafState] : path);
    }

    /// <summary>
    /// Previews an event against a supplied active shape without mutating runtime state or running lifecycle side effects.
    /// </summary>
    /// <remarks>
    /// Preview validates the definition and active shape, matches transitions, and evaluates guards because guard
    /// outcomes are required for permit/deny diagnostics. It does not run entry actions, exit actions, transition
    /// actions, transition behaviors, observers, persistence hooks, telemetry hooks, or completion cascades. Guards
    /// supplied by consumers should therefore be pure/idempotent for reliable dry-run behavior.
    /// </remarks>
    public ValueTask<TransitionPreviewResult<TState, TEvent>> PreviewAsync(
        ActiveStateShape<TState> activeStateShape,
        TEvent @event,
        CancellationToken cancellationToken = default)
    {
        return new TransitionPreviewPlanner<TState, TEvent>(this).PreviewAsync(activeStateShape, @event,
            cancellationToken);
    }

    /// <summary>Applies an event to caller-supplied state and returns a structured transition outcome.</summary>
    /// <remarks>
    ///     When <paramref name="observer" /> is omitted, execution preserves the dependency-free no-observer path.
    ///     Observer exceptions and cancellations are suppressed and never alter the returned outcome.
    /// </remarks>
    public async ValueTask<TransitionOutcome<TState, TEvent>> ApplyAsync(
        TState currentState,
        TEvent @event,
        CancellationToken cancellationToken = default,
        ITransitionObserver<TState, TEvent>? observer = null)
    {
        var executor = new TransitionExecutor<TState, TEvent>(this);
        var outcome = await executor.ApplyAsync(currentState, @event, cancellationToken, observer)
            .ConfigureAwait(false);
        if (!outcome.Committed) return outcome;

        return await ApplyCompletionCascadeAsync(outcome, cancellationToken, observer).ConfigureAwait(false);
    }

    private async ValueTask<TransitionOutcome<TState, TEvent>> ApplyCompletionCascadeAsync(
        TransitionOutcome<TState, TEvent> initialOutcome,
        CancellationToken cancellationToken,
        ITransitionObserver<TState, TEvent>? observer)
    {
        var tracker = new CompletionEpisodeTracker<TState>();
        var selector = new CompletionTransitionSelector<TState, TEvent>(this);
        var activeShape = initialOutcome.ActiveStateShape;
        var lastOutcome = initialOutcome;

        while (true)
        {
            TState completionScope;
            TState executorState;
            if (activeShape.IsParallel)
            {
                if (!ParallelCompletionEvaluator.IsComplete(this, activeShape) || activeShape.OwningCompositeState is null)
                    return lastOutcome;

                completionScope = activeShape.OwningCompositeState;
                executorState = completionScope;
            }
            else
            {
                var completion = HierarchyCompletionEvaluator.Evaluate(this, activeShape);
                if (completion is null || completion.Kind == NestedCompletionKind.Machine) return lastOutcome;

                completionScope = completion.CompletionScopeState!;
                executorState = activeShape.ActiveLeafState!;
            }

            if (tracker.IsRecognized(completionScope) || !selector.HasCandidates(completionScope)) return lastOutcome;

            var selection = await selector.SelectWithDiagnosticsAsync(executorState, completionScope, cancellationToken)
                .ConfigureAwait(false);
            if (selection.IsAmbiguous)
                return TransitionOutcome<TState, TEvent>.ValidationFailure(activeShape.ActiveLeafState!, default!,
                    new TransitionDiagnostics(selection.ConflictDiagnostic!.Message, TransitionLifecyclePhase.Matching,
                        conflictDiagnostics: [selection.ConflictDiagnostic]));

            if (selection.Selected is null)
            {
                tracker.MarkNoEligible(completionScope);
                return lastOutcome;
            }

            var selected = selection.Selected;
            tracker.MarkSelected(completionScope);
            var completionOutcome = await new TransitionExecutor<TState, TEvent>(this)
                .ApplyCompletionAsync(executorState, selected, cancellationToken, observer,
                    preTransitionActiveShape: activeShape)
                .ConfigureAwait(false);
            lastOutcome = completionOutcome;
            if (!completionOutcome.Committed) return lastOutcome;

            var before = activeShape;
            activeShape = completionOutcome.ActiveStateShape;
            tracker.ResetExitedScopes(this, before, activeShape);
        }
    }

    /// <summary>Creates a runtime context that owns its current state.</summary>
    public StateMachineRuntime<TState, TEvent> CreateRuntime(
        TState initialState,
        ConcurrencyMode concurrencyMode = ConcurrencyMode.Fast,
        ITransitionObserver<TState, TEvent>? observer = null)
    {
        return new StateMachineRuntime<TState, TEvent>(this, initialState, concurrencyMode, observer);
    }

    /// <summary>Creates a runtime context from a previously captured active-state snapshot.</summary>
    /// <exception cref="ActiveStateSnapshotValidationException{TState}">
    ///     Thrown when the snapshot is incompatible with this definition.
    /// </exception>
    public StateMachineRuntime<TState, TEvent> CreateRuntime(
        ActiveStateSnapshot<TState> snapshot,
        ConcurrencyMode concurrencyMode = ConcurrencyMode.Fast,
        ITransitionObserver<TState, TEvent>? observer = null)
    {
        var plan = ActiveStateSnapshotRestorePlanner.Plan(this, snapshot);
        if (!plan.IsValid)
            throw new ActiveStateSnapshotValidationException<TState>(plan.ValidationResult);

        return new StateMachineRuntime<TState, TEvent>(this, plan.ActiveStateShape!, concurrencyMode, observer);
    }

    /// <summary>Validates a supplied active-state snapshot without creating a runtime instance.</summary>
    public ActiveStateSnapshotValidationResult<TState> ValidateActiveStateSnapshot(ActiveStateSnapshot<TState> snapshot)
    {
        return ActiveStateSnapshotValidator.Validate(this, snapshot);
    }

    /// <summary>Creates a runtime context backed by application-owned state accessors.</summary>
    public ExternalStateMachineRuntime<TState, TEvent> CreateRuntime(
        IStateAccessor<TState> accessor,
        ConcurrencyMode concurrencyMode = ConcurrencyMode.Fast,
        ITransitionObserver<TState, TEvent>? observer = null)
    {
        return new ExternalStateMachineRuntime<TState, TEvent>(this, accessor, concurrencyMode, observer);
    }

    public DefinitionIntrospection<TState, TEvent> Introspect()
    {
        return new DefinitionIntrospection<TState, TEvent>(this);
    }

    /// <summary>Exports this reusable definition as structured graph data when validation has no errors.</summary>
    public GraphExportResult<TState, TEvent> ExportGraph()
    {
        return DefinitionGraphExporter.ExportGraph(this);
    }

    public IReadOnlyDictionary<string, object?> GetGraphMetadata()
    {
        return Metadata;
    }

    public ValueTask<IReadOnlyList<EventDefinition<TEvent>>> GetPermittedEventsAsync(TState state,
        CancellationToken cancellationToken = default)
    {
        return PermittedEventQuery.GetPermittedEventsAsync(this, state, cancellationToken);
    }

    private Dictionary<StateKey<TState>, StateRegionMembershipDefinition<TState>> BuildRegionMembershipLookup()
    {
        var result = new Dictionary<StateKey<TState>, StateRegionMembershipDefinition<TState>>();
        foreach (var region in ParallelRegions)
            foreach (var member in region.MemberStates.Concat(region.TerminalStates)
                         .Distinct(EqualityComparer<TState>.Default))
            {
                if (!ContainsState(member)) continue;

                var depth = GetActiveStatePath(member).StatesRootToLeaf.Count(s =>
                    !EqualityComparer<TState>.Default.Equals(s, region.OwnerCompositeState));
                result[new StateKey<TState>(member)] =
                    new StateRegionMembershipDefinition<TState>(member, region.RegionId, region.OwnerCompositeState, depth);
            }

        foreach (var state in States.Where(s => s.Hierarchy.HasParallelRegionMembership))
        {
            if (!_regionsById.TryGetValue(state.Hierarchy.ParallelRegionId!, out var region)) continue;

            result[new StateKey<TState>(state.Value)] =
                new StateRegionMembershipDefinition<TState>(state.Value, region.RegionId, region.OwnerCompositeState,
                    1);
        }

        return result;
    }

    private readonly record struct StateKey<T>(T Value);
}