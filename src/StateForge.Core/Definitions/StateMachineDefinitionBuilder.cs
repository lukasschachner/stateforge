using StateForge.Core.Execution;

namespace StateForge.Core.Definitions;

/// <summary>Fluent builder for immutable state machine definitions.</summary>
public sealed class StateMachineDefinitionBuilder<TState, TEvent>
{
    private readonly List<CompletionTransitionDefinition<TState, TEvent>> _completionTransitions = [];
    private readonly Dictionary<string, object?> _metadata = new(StringComparer.Ordinal);
    private readonly List<ParallelRegionBuilderState<TState>> _parallelRegions = [];
    private readonly List<StateDefinition<TState>> _states = [];
    private readonly List<TransitionDefinition<TState, TEvent>> _transitions = [];

    internal IReadOnlyList<StateDefinition<TState>> States => _states;
    internal IReadOnlyList<TransitionDefinition<TState, TEvent>> Transitions => _transitions;
    internal IReadOnlyList<CompletionTransitionDefinition<TState, TEvent>> CompletionTransitions => _completionTransitions;

    internal IReadOnlyList<ParallelRegionDefinition<TState>> ParallelRegions => _parallelRegions
        .Select((region, index) => region.ToDefinition(index))
        .ToArray();

    internal MetadataCollection Metadata => new(_metadata);

    public StateDefinitionBuilder<TState, TEvent> State(TState value)
    {
        var existing = _states.FindIndex(s => EqualityComparer<TState>.Default.Equals(s.Value, value));
        if (existing < 0) _states.Add(new StateDefinition<TState>(value));

        return new StateDefinitionBuilder<TState, TEvent>(this, value);
    }

    public StateMachineDefinitionBuilder<TState, TEvent> WithMetadata(string key, object? value)
    {
        _metadata[key] = value;
        return this;
    }

    public StateMachineDefinitionBuilder<TState, TEvent> ChildState(TState value, TState parentState)
    {
        State(value).ChildOf(parentState);
        State(parentState);
        return this;
    }

    public StateMachineDefinitionBuilder<TState, TEvent> CompositeState(TState value, TState initialChildState)
    {
        State(value).InitialChild(initialChildState);
        State(initialChildState).ChildOf(value);
        return this;
    }

    public StateMachineDefinitionBuilder<TState, TEvent> SetParent(TState childState, TState parentState)
    {
        State(childState).ChildOf(parentState);
        State(parentState);
        return this;
    }

    public StateMachineDefinitionBuilder<TState, TEvent> SetInitialChild(TState compositeState,
        TState initialChildState)
    {
        State(compositeState).InitialChild(initialChildState);
        State(initialChildState).ChildOf(compositeState);
        return this;
    }

    public StateMachineDefinitionBuilder<TState, TEvent> EnableHistory(TState compositeState,
        HistoryMode mode = HistoryMode.Shallow)
    {
        State(compositeState).WithHistory(mode);
        return this;
    }

    public StateMachineDefinitionBuilder<TState, TEvent> EnableHistory(TState compositeState, TState fallbackChildState,
        HistoryMode mode = HistoryMode.Shallow)
    {
        State(compositeState).WithHistory(mode, fallbackChildState);
        return this;
    }

    public ParallelCompositeDefinitionBuilder<TState, TEvent> ParallelComposite(TState compositeState)
    {
        return new ParallelCompositeDefinitionBuilder<TState, TEvent>(this, compositeState);
    }

    public StateMachineDefinitionBuilder<TState, TEvent> ParallelCompositeState(TState compositeState)
    {
        MarkParallelComposite(compositeState);
        return this;
    }

    public StateMachineDefinitionBuilder<TState, TEvent> ParallelRegion(TState compositeState, string name,
        TState initialState, params TState[] memberStates)
    {
        AddParallelRegion(compositeState, name, initialState, memberStates);
        return this;
    }

    public StateMachineDefinitionBuilder<TState, TEvent> ParallelRegion(TState compositeState, string name)
    {
        AddParallelRegion(compositeState, name, default, [], hasInitialState: false);
        return this;
    }

    public StateMachineDefinitionBuilder<TState, TEvent> ParallelRegion(TState compositeState, string name,
        Action<ParallelRegionDefinitionBuilder<TState, TEvent>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var regionId = AddParallelRegionBlock(compositeState, name);
        configure(new ParallelRegionDefinitionBuilder<TState, TEvent>(this, compositeState, name, regionId));
        return this;
    }

    public ParallelCompositeDefinitionBuilder<TState, TEvent> ParallelComposite(TState compositeState,
        Action<ParallelCompositeDefinitionBuilder<TState, TEvent>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var builder = new ParallelCompositeDefinitionBuilder<TState, TEvent>(this, compositeState);
        configure(builder);
        return builder;
    }

    internal void MarkTerminal(TState value)
    {
        var index = _states.FindIndex(s => EqualityComparer<TState>.Default.Equals(s.Value, value));
        if (index < 0)
        {
            _states.Add(new StateDefinition<TState>(value, true));
            return;
        }

        _states[index] = _states[index] with { IsTerminal = true };
    }

    internal void SetStateMetadata(TState value, string key, object? metadataValue)
    {
        var index = _states.FindIndex(s => EqualityComparer<TState>.Default.Equals(s.Value, value));
        if (index < 0)
        {
            _states.Add(new StateDefinition<TState>(value,
                metadata: MetadataCollection.Empty.With(key, metadataValue)));
            return;
        }

        var state = _states[index];
        _states[index] = state with { Metadata = state.Metadata.With(key, metadataValue) };
    }

    internal void SetParentState(TState value, TState parentState)
    {
        var index = EnsureState(value);
        var state = _states[index];
        _states[index] = state with { Hierarchy = state.Hierarchy.WithParent(parentState) };
        EnsureState(parentState);
    }

    internal void SetInitialChildState(TState value, TState initialChildState)
    {
        var index = EnsureState(value);
        var state = _states[index];
        _states[index] = state with { Hierarchy = state.Hierarchy.WithInitialChild(initialChildState) };
        SetParentState(initialChildState, value);
    }

    internal void SetHistory(TState value, HistoryMode mode, bool hasFallback = false, TState? fallbackState = default)
    {
        var index = EnsureState(value);
        var state = _states[index];
        _states[index] = state with { Hierarchy = state.Hierarchy.WithHistory(mode, hasFallback, fallbackState) };
        if (hasFallback) EnsureState(fallbackState!);
    }

    internal void MarkParallelComposite(TState value)
    {
        var index = EnsureState(value);
        var state = _states[index];
        _states[index] = state with
        {
            Hierarchy = state.Hierarchy.AsParallelComposite(),
            Metadata = state.Metadata.With(ParallelRegionMetadataKeys.IsParallelComposite, true)
        };
    }

    internal string AddParallelRegion(TState compositeState, string name, TState? initialState,
        IEnumerable<TState>? memberStates = null, IEnumerable<TState>? terminalStates = null,
        MetadataCollection? metadata = null, bool hasInitialState = true)
    {
        MarkParallelComposite(compositeState);
        var order = _parallelRegions.Count(r =>
            EqualityComparer<TState>.Default.Equals(r.OwnerCompositeState, compositeState));
        var regionId = $"region-{_parallelRegions.Count:000}";
        var members = new List<TState>();
        if (hasInitialState) members.Add(initialState!);
        if (memberStates is not null) members.AddRange(memberStates);
        var uniqueMembers = members.Distinct(EqualityComparer<TState>.Default).ToArray();
        _parallelRegions.Add(new ParallelRegionBuilderState<TState>(regionId, name, compositeState, order,
            hasInitialState, initialState, uniqueMembers, (terminalStates ?? []).ToArray(),
            metadata ?? MetadataCollection.Empty));
        foreach (var member in uniqueMembers)
        {
            SetParentState(member, compositeState);
            AssignStateToRegion(member, compositeState, regionId, name);
        }

        foreach (var terminal in terminalStates ?? [])
        {
            MarkTerminal(terminal);
            if (!uniqueMembers.Contains(terminal, EqualityComparer<TState>.Default))
            {
                SetParentState(terminal, compositeState);
                AssignStateToRegion(terminal, compositeState, regionId, name);
            }
        }

        return regionId;
    }

    internal string AddParallelRegionBlock(TState compositeState, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Parallel region name must be non-blank.", nameof(name));

        return AddParallelRegion(compositeState, name, default, [], hasInitialState: false);
    }

    internal StateDefinitionBuilder<TState, TEvent> AddStateToParallelRegion(string regionId, TState state)
    {
        var region = GetMutableRegion(regionId);
        AddRegionMember(regionId, state);
        AssignStateToRegion(state, region.OwnerCompositeState, region.RegionId, region.Name);
        return new StateDefinitionBuilder<TState, TEvent>(this, state);
    }

    internal StateDefinitionBuilder<TState, TEvent> SetParallelRegionInitialState(string regionId, TState state)
    {
        AddStateToParallelRegion(regionId, state);
        UpdateParallelRegion(regionId, region => region with { HasInitialState = true, InitialState = state });
        return new StateDefinitionBuilder<TState, TEvent>(this, state);
    }

    internal StateDefinitionBuilder<TState, TEvent> AddParallelRegionTerminalState(string regionId, TState state)
    {
        AddStateToParallelRegion(regionId, state);
        AddRegionTerminal(regionId, state);
        MarkTerminal(state);
        return new StateDefinitionBuilder<TState, TEvent>(this, state);
    }

    internal void SetParallelRegionMetadata(string regionId, string key, object? value)
    {
        UpdateParallelRegion(regionId, region => region with { Metadata = region.Metadata.With(key, value) });
    }

    internal void AssignStateToRegion(TState value, TState compositeState, string regionName)
    {
        var region = _parallelRegions.FirstOrDefault(r =>
            EqualityComparer<TState>.Default.Equals(r.OwnerCompositeState, compositeState) &&
            string.Equals(r.Name, regionName, StringComparison.Ordinal));
        AssignStateToRegion(value, compositeState, region?.RegionId ?? $"unresolved-region:{regionName}", regionName);
    }

    private void AssignStateToRegion(TState value, TState compositeState, string regionId, string regionName)
    {
        var index = EnsureState(value);
        var state = _states[index];
        _states[index] = state with
        {
            Hierarchy = state.Hierarchy.InParallelRegion(regionId),
            Metadata = state.Metadata
                .With(ParallelRegionMetadataKeys.RegionId, regionId)
                .With(ParallelRegionMetadataKeys.RegionName, regionName)
                .With(ParallelRegionMetadataKeys.OwnerCompositeState, compositeState)
        };
        if (!EqualityComparer<TState>.Default.Equals(value, compositeState)) SetParentState(value, compositeState);
    }

    private ParallelRegionBuilderState<TState> GetMutableRegion(string regionId)
    {
        return _parallelRegions.First(r => string.Equals(r.RegionId, regionId, StringComparison.Ordinal));
    }

    private void AddRegionMember(string regionId, TState state)
    {
        UpdateParallelRegion(regionId, region => region.MemberStates.Contains(state, EqualityComparer<TState>.Default)
            ? region
            : region with { MemberStates = region.MemberStates.Concat([state]).ToArray() });
    }

    private void AddRegionTerminal(string regionId, TState state)
    {
        UpdateParallelRegion(regionId, region => region.TerminalStates.Contains(state, EqualityComparer<TState>.Default)
            ? region
            : region with { TerminalStates = region.TerminalStates.Concat([state]).ToArray() });
    }

    private void UpdateParallelRegion(string regionId,
        Func<ParallelRegionBuilderState<TState>, ParallelRegionBuilderState<TState>> update)
    {
        var index = _parallelRegions.FindIndex(r => string.Equals(r.RegionId, regionId, StringComparison.Ordinal));
        if (index < 0) throw new InvalidOperationException($"Parallel region '{regionId}' was not found.");

        _parallelRegions[index] = update(_parallelRegions[index]);
    }

    internal void AddStateAction(TState value, StateActionDefinition<TState> action)
    {
        var index = _states.FindIndex(s => EqualityComparer<TState>.Default.Equals(s.Value, value));
        if (index < 0)
        {
            var newState = action.Kind == ActionKind.Entry
                ? new StateDefinition<TState>(value, entryActions: [action])
                : new StateDefinition<TState>(value, exitActions: [action]);
            _states.Add(newState);
            return;
        }

        var state = _states[index];
        if (action.Kind == ActionKind.Entry)
        {
            _states[index] = state with { EntryActions = state.EntryActions.Concat([action]).ToArray() };
            return;
        }

        _states[index] = state with { ExitActions = state.ExitActions.Concat([action]).ToArray() };
    }

    internal void AddTransition(TransitionDefinition<TState, TEvent> transition)
    {
        _transitions.Add(transition);
    }

    internal void AddCompletionTransition(CompletionTransitionDefinition<TState, TEvent> transition)
    {
        EnsureState(transition.SourceState);
        _completionTransitions.Add(new CompletionTransitionDefinition<TState, TEvent>(
            transition.SourceState,
            transition.TargetState,
            transition.Kind,
            transition.Conditions,
            transition.Behaviors,
            transition.Metadata,
            transition.TransitionActions,
            _completionTransitions.Count));
    }

    private int EnsureState(TState value)
    {
        var index = _states.FindIndex(s => EqualityComparer<TState>.Default.Equals(s.Value, value));
        if (index >= 0) return index;

        _states.Add(new StateDefinition<TState>(value));
        return _states.Count - 1;
    }

    internal sealed record ParallelRegionBuilderState<T>(
        string RegionId,
        string Name,
        T OwnerCompositeState,
        int Order,
        bool HasInitialState,
        T? InitialState,
        IReadOnlyList<T> MemberStates,
        IReadOnlyList<T> TerminalStates,
        MetadataCollection Metadata)
    {
        public ParallelRegionDefinition<T> ToDefinition(int _)
        {
            return new ParallelRegionDefinition<T>(RegionId, Name, OwnerCompositeState, Order, HasInitialState,
                InitialState, MemberStates,
                TerminalStates, Metadata);
        }
    }
}

/// <summary>Fluent state configuration.</summary>
public sealed class StateDefinitionBuilder<TState, TEvent>
{
    private readonly StateMachineDefinitionBuilder<TState, TEvent> _builder;

    internal StateDefinitionBuilder(StateMachineDefinitionBuilder<TState, TEvent> builder, TState state)
    {
        _builder = builder;
        State = state;
    }

    public TState State { get; }

    public StateDefinitionBuilder<TState, TEvent> Terminal()
    {
        _builder.MarkTerminal(State);
        return this;
    }

    public StateDefinitionBuilder<TState, TEvent> WithMetadata(string key, object? value)
    {
        _builder.SetStateMetadata(State, key, value);
        return this;
    }

    public StateDefinitionBuilder<TState, TEvent> ChildOf(TState parentState)
    {
        _builder.SetParentState(State, parentState);
        return this;
    }

    public StateDefinitionBuilder<TState, TEvent> WithParent(TState parentState)
    {
        return ChildOf(parentState);
    }

    public StateDefinitionBuilder<TState, TEvent> InitialChild(TState initialChildState)
    {
        _builder.SetInitialChildState(State, initialChildState);
        return this;
    }

    public StateDefinitionBuilder<TState, TEvent> WithInitialChild(TState initialChildState)
    {
        return InitialChild(initialChildState);
    }

    public StateDefinitionBuilder<TState, TEvent> Composite(TState initialChildState)
    {
        return InitialChild(initialChildState);
    }

    public StateDefinitionBuilder<TState, TEvent> WithHistory(HistoryMode mode = HistoryMode.Shallow)
    {
        _builder.SetHistory(State, mode);
        return this;
    }

    public StateDefinitionBuilder<TState, TEvent> WithHistory(HistoryMode mode, TState fallbackChildState)
    {
        _builder.SetHistory(State, mode, true, fallbackChildState);
        return this;
    }

    public StateDefinitionBuilder<TState, TEvent> WithHistory(TState fallbackChildState,
        HistoryMode mode = HistoryMode.Shallow)
    {
        _builder.SetHistory(State, mode, true, fallbackChildState);
        return this;
    }

    public StateDefinitionBuilder<TState, TEvent> WithShallowHistory()
    {
        return WithHistory();
    }

    public StateDefinitionBuilder<TState, TEvent> WithShallowHistory(TState fallbackChildState)
    {
        return WithHistory(HistoryMode.Shallow, fallbackChildState);
    }

    public StateDefinitionBuilder<TState, TEvent> WithDeepHistory()
    {
        return WithHistory(HistoryMode.Deep);
    }

    public StateDefinitionBuilder<TState, TEvent> WithDeepHistory(TState fallbackChildState)
    {
        return WithHistory(HistoryMode.Deep, fallbackChildState);
    }

    public StateDefinitionBuilder<TState, TEvent> ParallelComposite()
    {
        _builder.MarkParallelComposite(State);
        return this;
    }

    public StateDefinitionBuilder<TState, TEvent> InRegion(TState compositeState, string regionName)
    {
        _builder.AssignStateToRegion(State, compositeState, regionName);
        return this;
    }

    public StateDefinitionBuilder<TState, TEvent> InParallelRegion(TState compositeState, string regionName)
    {
        return InRegion(compositeState, regionName);
    }

    public StateDefinitionBuilder<TState, TEvent> OnEntry(Action<ActionExecutionContext<TState, TEvent>> action,
        string? displayName = null, MetadataCollection? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        var order = CurrentActionCount(ActionKind.Entry);
        _builder.AddStateAction(State,
            StateActionDefinition<TState>.FromSync(ActionKind.Entry, order, action, displayName, metadata));
        return this;
    }

    public StateDefinitionBuilder<TState, TEvent> OnEntryAsync(
        Func<ActionExecutionContext<TState, TEvent>, CancellationToken, ValueTask> action, string? displayName = null,
        MetadataCollection? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        var order = CurrentActionCount(ActionKind.Entry);
        _builder.AddStateAction(State,
            StateActionDefinition<TState>.FromAsync(ActionKind.Entry, order, action, displayName, metadata));
        return this;
    }

    public StateDefinitionBuilder<TState, TEvent> OnExit(Action<ActionExecutionContext<TState, TEvent>> action,
        string? displayName = null, MetadataCollection? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        var order = CurrentActionCount(ActionKind.Exit);
        _builder.AddStateAction(State,
            StateActionDefinition<TState>.FromSync(ActionKind.Exit, order, action, displayName, metadata));
        return this;
    }

    public StateDefinitionBuilder<TState, TEvent> OnExitAsync(
        Func<ActionExecutionContext<TState, TEvent>, CancellationToken, ValueTask> action, string? displayName = null,
        MetadataCollection? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        var order = CurrentActionCount(ActionKind.Exit);
        _builder.AddStateAction(State,
            StateActionDefinition<TState>.FromAsync(ActionKind.Exit, order, action, displayName, metadata));
        return this;
    }

    public TransitionDefinitionBuilder<TState, TEvent> On(TEvent eventValue)
    {
        return new TransitionDefinitionBuilder<TState, TEvent>(_builder, this,
            EventDefinition<TEvent>.ForValue(eventValue));
    }

    public TransitionDefinitionBuilder<TState, TEvent> On<TSpecificEvent>() where TSpecificEvent : TEvent
    {
        return new TransitionDefinitionBuilder<TState, TEvent>(_builder, this,
            EventDefinition<TEvent>.ForType<TSpecificEvent>());
    }

    public CompletionTransitionDefinitionBuilder<TState, TEvent> OnCompletion()
    {
        return new CompletionTransitionDefinitionBuilder<TState, TEvent>(_builder, this);
    }

    private int CurrentActionCount(ActionKind kind)
    {
        var state = _builder.States.FirstOrDefault(s => EqualityComparer<TState>.Default.Equals(s.Value, State));
        return kind == ActionKind.Entry ? state?.EntryActions.Count ?? 0 : state?.ExitActions.Count ?? 0;
    }
}

/// <summary>Fluent configuration for one parallel composite and its regions.</summary>
public sealed class ParallelCompositeDefinitionBuilder<TState, TEvent>
{
    private readonly StateMachineDefinitionBuilder<TState, TEvent> _builder;

    internal ParallelCompositeDefinitionBuilder(StateMachineDefinitionBuilder<TState, TEvent> builder,
        TState compositeState)
    {
        _builder = builder;
        CompositeState = compositeState;
        _builder.MarkParallelComposite(compositeState);
    }

    public TState CompositeState { get; }

    public ParallelCompositeDefinitionBuilder<TState, TEvent> WithHistory(HistoryMode mode = HistoryMode.Shallow)
    {
        _builder.SetHistory(CompositeState, mode);
        return this;
    }

    public ParallelCompositeDefinitionBuilder<TState, TEvent> WithShallowHistory()
    {
        return WithHistory();
    }

    public ParallelCompositeDefinitionBuilder<TState, TEvent> WithDeepHistory()
    {
        return WithHistory(HistoryMode.Deep);
    }

    public ParallelCompositeDefinitionBuilder<TState, TEvent> Region(string name)
    {
        _builder.AddParallelRegion(CompositeState, name, default, [], hasInitialState: false);
        return this;
    }

    public ParallelCompositeDefinitionBuilder<TState, TEvent> Region(string name,
        Action<ParallelRegionDefinitionBuilder<TState, TEvent>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var regionId = _builder.AddParallelRegionBlock(CompositeState, name);
        configure(new ParallelRegionDefinitionBuilder<TState, TEvent>(_builder, CompositeState, name, regionId));
        return this;
    }

    public ParallelCompositeDefinitionBuilder<TState, TEvent> Region(string name, TState initialState,
        params TState[] memberStates)
    {
        _builder.AddParallelRegion(CompositeState, name, initialState, memberStates);
        return this;
    }

    public ParallelCompositeDefinitionBuilder<TState, TEvent> Region(string name, TState initialState,
        IEnumerable<TState> memberStates, IEnumerable<TState>? terminalStates = null,
        MetadataCollection? metadata = null)
    {
        _builder.AddParallelRegion(CompositeState, name, initialState, memberStates, terminalStates, metadata);
        return this;
    }

    public ParallelCompletionTransitionDefinitionBuilder<TState, TEvent> OnCompletion()
    {
        return new ParallelCompletionTransitionDefinitionBuilder<TState, TEvent>(_builder, this);
    }
}

/// <summary>Fluent configuration for one named region of a parallel composite.</summary>
public sealed class ParallelRegionDefinitionBuilder<TState, TEvent>
{
    private readonly StateMachineDefinitionBuilder<TState, TEvent> _builder;

    internal ParallelRegionDefinitionBuilder(StateMachineDefinitionBuilder<TState, TEvent> builder, TState compositeState,
        string regionName, string regionId)
    {
        _builder = builder;
        CompositeState = compositeState;
        RegionName = regionName;
        RegionId = regionId;
    }

    public TState CompositeState { get; }

    public string RegionName { get; }

    public string RegionId { get; }

    public StateDefinitionBuilder<TState, TEvent> State(TState state)
    {
        return _builder.AddStateToParallelRegion(RegionId, state);
    }

    public StateDefinitionBuilder<TState, TEvent> Initial(TState state)
    {
        return _builder.SetParallelRegionInitialState(RegionId, state);
    }

    public StateDefinitionBuilder<TState, TEvent> Terminal(TState state)
    {
        return _builder.AddParallelRegionTerminalState(RegionId, state);
    }

    public ParallelRegionDefinitionBuilder<TState, TEvent> WithMetadata(string key, object? value)
    {
        _builder.SetParallelRegionMetadata(RegionId, key, value);
        return this;
    }
}
