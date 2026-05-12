using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

/// <summary>Deterministically matches a transition for current state and event.</summary>
internal sealed class TransitionMatcher<TState, TEvent>
{
    private readonly StateMachineDefinition<TState, TEvent> _definition;

    public TransitionMatcher(StateMachineDefinition<TState, TEvent> definition)
    {
        _definition = definition;
    }

    public TransitionDefinition<TState, TEvent>? Match(TState currentState, TEvent @event)
    {
        if (_definition.HasHierarchy)
            return new HierarchicalTransitionResolver<TState, TEvent>(_definition).Match(currentState, @event);

        foreach (var transition in _definition.Transitions)
            if (EqualityComparer<TState>.Default.Equals(transition.SourceState, currentState) &&
                transition.Event.Matches(@event))
                return transition;

        return null;
    }
}