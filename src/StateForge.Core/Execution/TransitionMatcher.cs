using StateForge.Core.Definitions;

namespace StateForge.Core.Execution;

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

    public IReadOnlyList<TransitionDefinition<TState, TEvent>> EnumerateEventCandidates(TState currentState,
        TEvent @event)
    {
        var sourcePath = _definition.HasHierarchy
            ? _definition.GetActiveStatePath(currentState).StatesRootToLeaf
            : [currentState];

        return _definition.Transitions
            .Where(transition => transition.Event.Matches(@event))
            .Where(transition => sourcePath.Contains(transition.SourceState, EqualityComparer<TState>.Default))
            .ToArray();
    }
}