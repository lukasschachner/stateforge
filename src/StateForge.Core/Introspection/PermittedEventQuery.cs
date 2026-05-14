using StateForge.Core.Definitions;
using StateForge.Core.Execution;

namespace StateForge.Core.Introspection;

/// <summary>Queries events declared as permitted from a supplied state.</summary>
internal static class PermittedEventQuery
{
    public static ValueTask<IReadOnlyList<EventDefinition<TEvent>>> GetPermittedEventsAsync<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        TState state,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sourceStates = definition.HasHierarchy
            ? definition.GetActiveStatePath(state).StatesRootToLeaf
            : [state];
        return ValueTask.FromResult(GetPermittedEvents(definition, sourceStates));
    }

    public static ValueTask<IReadOnlyList<EventDefinition<TEvent>>> GetPermittedEventsAsync<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        ActiveStateShape<TState> activeShape,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(activeShape);

        if (!activeShape.IsParallel)
            return GetPermittedEventsAsync(definition, activeShape.ActiveLeafState!, cancellationToken);

        var sourceStates = activeShape.ActiveRegions
            .SelectMany(region => region.ActivePath.StatesRootToLeaf)
            .Distinct(EqualityComparer<TState>.Default)
            .ToArray();
        return ValueTask.FromResult(GetPermittedEvents(definition, sourceStates));
    }

    private static IReadOnlyList<EventDefinition<TEvent>> GetPermittedEvents<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        IEnumerable<TState> sourceStates)
    {
        var sourceStateSet = new HashSet<TState>(sourceStates, EqualityComparer<TState>.Default);
        return definition.Transitions
            .Where(t => sourceStateSet.Contains(t.SourceState))
            .Select(t => t.Event)
            .GroupBy(e => e.Identity)
            .Select(g => g.First())
            .ToArray();
    }
}