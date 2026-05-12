using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

internal sealed class ParallelDispatchPlanner<TState, TEvent>
{
    private readonly StateMachineDefinition<TState, TEvent> _definition;

    public ParallelDispatchPlanner(StateMachineDefinition<TState, TEvent> definition)
    {
        _definition = definition;
    }

    public ParallelDispatchPlan<TState, TEvent> Plan(ActiveStateShape<TState> shape, TEvent @event)
    {
        var selected = new ParallelTransitionResolver<TState, TEvent>(_definition).Resolve(shape, @event)
            .Select(x => x.Transition).ToArray();
        var parent = shape.OwningCompositeState is null
            ? null
            : new TransitionMatcher<TState, TEvent>(_definition).Match(shape.OwningCompositeState, @event);
        var postShape = selected.Length == 0 ? shape : shape;
        var parentIsCompletion = parent is not null && selected.Length > 0;
        var conflicts = ParallelConflictDetector.Detect(_definition, selected, parent, parentIsCompletion);
        return new ParallelDispatchPlan<TState, TEvent>(@event, shape, selected, parent, conflicts,
            conflicts.Count == 0 ? postShape : null);
    }
}