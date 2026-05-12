using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;

namespace StateMachineLibrary.Core.Introspection;

/// <summary>Read-only queries over a state machine definition.</summary>
public sealed class DefinitionIntrospection<TState, TEvent>
{
    private readonly StateMachineDefinition<TState, TEvent> _definition;

    public DefinitionIntrospection(StateMachineDefinition<TState, TEvent> definition)
    {
        _definition = definition;
    }

    public IReadOnlyList<StateDefinition<TState>> DeclaredStates => _definition.States;
    public IReadOnlyList<EventDefinition<TEvent>> DeclaredEvents => _definition.Events;
    public IReadOnlyList<TransitionDefinition<TState, TEvent>> DeclaredTransitions => _definition.Transitions;
    public IReadOnlyList<CompletionTransitionDefinition<TState, TEvent>> DeclaredCompletionTransitions =>
        _definition.CompletionTransitions;
    public IReadOnlyList<StateDefinition<TState>> TerminalStates => _definition.TerminalStates;
    public IReadOnlyDictionary<string, object?> GraphMetadata => _definition.Metadata;
    public bool HasHierarchy => _definition.HasHierarchy;
    public bool HasHistory => _definition.HasHistory;
    public bool HasParallelRegions => _definition.HasParallelRegions;
    public IReadOnlyList<StateDefinition<TState>> HistoryEnabledStates => _definition.HistoryEnabledStates;
    public IReadOnlyList<ParallelRegionDefinition<TState>> ParallelRegions => _definition.ParallelRegions;

    public IReadOnlyList<ParallelHistoryDefinitionMetadata<TState>> ParallelHistoryDefinitions => _definition
        .HistoryEnabledStates
        .Where(state => _definition.IsParallelComposite(state.Value))
        .Select(state => new ParallelHistoryDefinitionMetadata<TState>(
            state.Value,
            state.HistoryMode,
            _definition.GetParallelRegions(state.Value)
                .OrderBy(region => region.Order)
                .Select(region =>
                    new ParallelHistoryRegionFallbackMetadata<TState>(region.RegionId, region.Name, region.Order,
                        region.InitialState))
                .ToArray()))
        .ToArray();

    public HistoryMode GetParallelHistoryMode(TState compositeState)
    {
        return _definition.TryGetHistoryDefinition(compositeState, out var state) &&
               _definition.IsParallelComposite(compositeState)
            ? state.HistoryMode
            : HistoryMode.None;
    }

    public IReadOnlyList<StateDefinition<TState>> ChildrenOf(TState parentState)
    {
        return _definition.GetChildren(parentState);
    }

    public bool IsParallelComposite(TState state)
    {
        return _definition.IsParallelComposite(state);
    }

    public IReadOnlyList<ParallelRegionDefinition<TState>> RegionsOf(TState compositeState)
    {
        return _definition.GetParallelRegions(compositeState);
    }

    public bool TryGetRegionMembership(TState state, out StateRegionMembershipDefinition<TState> membership)
    {
        return _definition.TryGetRegionMembership(state, out membership);
    }

    public ActiveStatePath<TState> GetActiveStatePath(TState activeLeafState)
    {
        return _definition.GetActiveStatePath(activeLeafState);
    }

    public ActiveStateSnapshotKind GetActiveStateSnapshotKind(TState activeLeafState)
    {
        return _definition.IsParallelComposite(activeLeafState)
            ? ActiveStateSnapshotKind.Parallel
            : _definition.GetActiveStatePath(activeLeafState).Depth > 1
                ? ActiveStateSnapshotKind.Hierarchical
                : ActiveStateSnapshotKind.SingleLeaf;
    }

    public GraphExportResult<TState, TEvent> ExportGraph()
    {
        return DefinitionGraphExporter.ExportGraph(_definition);
    }

    public IReadOnlyList<TransitionDefinition<TState, TEvent>> OutgoingTransitions(TState state)
    {
        return _definition.Transitions.Where(t => EqualityComparer<TState>.Default.Equals(t.SourceState, state))
            .ToArray();
    }

    public IReadOnlyList<CompletionTransitionDefinition<TState, TEvent>> OutgoingCompletionTransitions(TState state)
    {
        return _definition.CompletionTransitions.Where(t => EqualityComparer<TState>.Default.Equals(t.SourceState, state))
            .ToArray();
    }
}