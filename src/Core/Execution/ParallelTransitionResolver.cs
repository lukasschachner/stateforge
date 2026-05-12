using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

internal sealed class ParallelTransitionResolver<TState, TEvent>
{
    private readonly StateMachineDefinition<TState, TEvent> _definition;

    public ParallelTransitionResolver(StateMachineDefinition<TState, TEvent> definition)
    {
        _definition = definition;
    }

    public IReadOnlyList<(ActiveRegionEntry<TState> Region, TransitionDefinition<TState, TEvent> Transition)> Resolve(
        ActiveStateShape<TState> shape, TEvent @event)
    {
        var matcher = new TransitionMatcher<TState, TEvent>(_definition);
        return shape.ActiveRegions
            .Select(entry => (Region: entry, Transition: matcher.Match(entry.ActiveLeafState, @event)))
            .Where(x => x.Transition is not null && IsRegionalTransition(x.Region, x.Transition!))
            .Select(x => (x.Region, x.Transition!))
            .ToArray();
    }

    private bool IsRegionalTransition(ActiveRegionEntry<TState> region, TransitionDefinition<TState, TEvent> transition)
    {
        return _definition.TryGetRegionMembership(transition.SourceState, out var sourceMembership)
               && StringComparer.Ordinal.Equals(sourceMembership.RegionId, region.RegionId);
    }
}