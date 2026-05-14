using StateForge.Core.Definitions;

namespace StateForge.Core.Execution;

internal static class RuntimeTransitionHelpers
{
    public static IReadOnlyList<ActiveRegionEntry<TState>> PlanPostRegionalEntries<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        ActiveStateShape<TState> activeShape,
        IReadOnlyList<(ActiveRegionEntry<TState> Region, TransitionDefinition<TState, TEvent> Transition)> matches)
    {
        var entries = activeShape.ActiveRegions.ToDictionary(entry => entry.RegionId, StringComparer.Ordinal);
        foreach (var (region, transition) in matches)
        {
            var target = transition.Kind == TransitionKind.Internal
                ? region.ActiveLeafState
                : InitialChildResolver.ResolveTargetLeaf(definition, transition.TargetState);
            var regionDefinition = definition.GetParallelRegions(activeShape.OwningCompositeState!)
                .First(candidate => candidate.RegionId == region.RegionId);
            entries[region.RegionId] = new ActiveRegionEntry<TState>(
                region.RegionId,
                region.RegionName,
                target,
                definition.GetActiveStatePath(target),
                regionDefinition.TerminalStates.Contains(target, EqualityComparer<TState>.Default));
        }

        return definition.GetParallelRegions(activeShape.OwningCompositeState!)
            .Select(region => entries[region.RegionId])
            .ToArray();
    }

    public static void CommitHistory<TState>(
        IDictionary<TState, CompositeHistoryRecord<TState>> destination,
        IDictionary<TState, CompositeHistoryRecord<TState>> source)
    {
        destination.Clear();
        foreach (var item in source) destination[item.Key] = item.Value;
    }

    public static string? ResolveEventIdentity<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        TEvent @event)
    {
        foreach (var eventDefinition in definition.Events)
            if (eventDefinition.Matches(@event))
                return eventDefinition.Identity;

        return null;
    }

    public static IReadOnlyList<CompositeHistorySnapshot<TState>> CreateHistorySnapshots<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        IReadOnlyDictionary<TState, CompositeHistoryRecord<TState>> historyRecords)
    {
        return definition.HistoryEnabledStates
            .Select(state =>
            {
                historyRecords.TryGetValue(state.Value, out var record);
                definition.TryGetEffectiveHistoryFallback(state.Value, out var fallback);
                return new CompositeHistorySnapshot<TState>(
                    state.Value,
                    record is not null,
                    record is null ? default : record.LastActiveDirectChildState,
                    record is null ? default : record.LastActiveDescendantLeafState,
                    fallback,
                    record?.LastUpdatedSequence ?? 0);
            })
            .ToArray();
    }

    public static async ValueTask RunParallelExitActionsAsync<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        ActiveStateShape<TState> activeShape,
        TransitionDefinition<TState, TEvent> parentTransition,
        TEvent @event,
        CancellationToken cancellationToken)
    {
        var runner = new TransitionActionRunner<TState, TEvent>();
        foreach (var region in HierarchyEntryExitPlanner.ExitOrder(activeShape.ActiveRegions))
        {
            var context = new ActionExecutionContext<TState, TEvent>(definition, region.ActiveLeafState,
                parentTransition.TargetState, @event, parentTransition, TransitionLifecyclePhase.Exit,
                cancellationToken, regionId: region.RegionId, regionName: region.RegionName,
                triggerKind: parentTransition.TriggerKind);
            foreach (var state in region.ActivePath.StatesRootToLeaf.Reverse().TakeWhile(state =>
                         !EqualityComparer<TState>.Default.Equals(state, activeShape.OwningCompositeState)))
                await runner.RunStateActionsAsync(definition.FindState(state), ActionKind.Exit, parentTransition,
                    context, cancellationToken).ConfigureAwait(false);
        }
    }
}
