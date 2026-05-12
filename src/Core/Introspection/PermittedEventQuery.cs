using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Introspection;

/// <summary>Queries events declared as permitted from a supplied state.</summary>
internal static class PermittedEventQuery
{
    public static ValueTask<IReadOnlyList<EventDefinition<TEvent>>> GetPermittedEventsAsync<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        TState state,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var comparer = EqualityComparer<TState>.Default;
        var sourceStates = definition.HasHierarchy
            ? definition.GetActiveStatePath(state).StatesRootToLeaf.Reverse().ToArray()
            : [state];
        IReadOnlyList<EventDefinition<TEvent>> events = definition.Transitions
            .Where(t => sourceStates.Any(s => comparer.Equals(t.SourceState, s)))
            .Select(t => t.Event)
            .GroupBy(e => e.Identity)
            .Select(g => g.First())
            .ToArray();
        return ValueTask.FromResult(events);
    }
}