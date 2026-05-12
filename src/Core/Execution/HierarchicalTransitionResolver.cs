using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

internal sealed class HierarchicalTransitionResolver<TState, TEvent>
{
    private readonly StateMachineDefinition<TState, TEvent> _definition;

    public HierarchicalTransitionResolver(StateMachineDefinition<TState, TEvent> definition)
    {
        _definition = definition;
    }

    public TransitionDefinition<TState, TEvent>? Match(TState currentLeafState, TEvent @event)
    {
        var comparer = EqualityComparer<TState>.Default;
        var path = _definition.GetActiveStatePath(currentLeafState).StatesRootToLeaf;

        for (var i = path.Count - 1; i >= 0; i--)
        {
            var resolutionState = path[i];
            var transition = _definition.Transitions.FirstOrDefault(t =>
                comparer.Equals(t.SourceState, resolutionState) && t.Event.Matches(@event));
            if (transition is not null) return transition;
        }

        return null;
    }
}