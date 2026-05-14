using StateForge.Core.Definitions;

namespace StateForge.Core.Validation;

internal static class ParallelRegionReachabilityValidator
{
    public static void Validate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        ICollection<ValidationFinding> findings)
    {
        foreach (var region in definition.ParallelRegions.Where(r => r.HasInitialState))
        {
            var members = region.MemberStates.Concat(region.TerminalStates).Distinct(EqualityComparer<TState>.Default)
                .ToArray();
            var reachable = new HashSet<TState>(EqualityComparer<TState>.Default) { region.InitialState! };
            var changed = true;
            while (changed)
            {
                changed = false;
                foreach (var transition in definition.Transitions.Where(t =>
                             reachable.Contains(t.SourceState) &&
                             members.Contains(t.TargetState, EqualityComparer<TState>.Default)))
                    changed |= reachable.Add(transition.TargetState);
            }

            foreach (var member in members.Where(m => !reachable.Contains(m)))
                findings.Add(new ValidationFinding(ValidationSeverity.Warning,
                    ParallelValidationCodes.UnreachableRegionalState,
                    $"State '{member}' is not reachable from initial state '{region.InitialState}' in region '{region.Name}'.",
                    $"state:{member}", "Add a regional transition from a reachable state or remove the unused state.",
                    region.OwnerCompositeState, region.RegionId, region.Name));
        }
    }
}