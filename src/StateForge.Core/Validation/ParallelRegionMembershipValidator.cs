using StateForge.Core.Definitions;

namespace StateForge.Core.Validation;

internal static class ParallelRegionMembershipValidator
{
    public static void Validate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        ICollection<ValidationFinding> findings)
    {
        foreach (var compositeGroup in definition.ParallelRegions.GroupBy(r => r.OwnerCompositeState,
                     EqualityComparer<TState>.Default))
        {
            var all = compositeGroup
                .SelectMany(r => r.MemberStates.Concat(r.TerminalStates).Select(s => (State: s, Region: r))).ToArray();
            foreach (var duplicate in
                     all.GroupBy(x => x.State, EqualityComparer<TState>.Default).Where(g =>
                         g.Select(x => x.Region.RegionId).Distinct(StringComparer.Ordinal).Count() > 1))
                findings.Add(new ValidationFinding(ValidationSeverity.Error, ParallelValidationCodes.InvalidMembership,
                    $"State '{duplicate.Key}' is assigned to multiple regions under parallel composite '{compositeGroup.Key}'.",
                    $"state:{duplicate.Key}", "Assign each state to exactly one sibling region.", compositeGroup.Key));

            var regionsById = compositeGroup.ToDictionary(r => r.RegionId, StringComparer.Ordinal);
            foreach (var state in definition.States.Where(s => s.Hierarchy.HasParallelRegionMembership))
            {
                if (!regionsById.TryGetValue(state.Hierarchy.ParallelRegionId!, out var assignedRegion)) continue;

                var listedRegions = all
                    .Where(x => EqualityComparer<TState>.Default.Equals(x.State, state.Value))
                    .Select(x => x.Region)
                    .DistinctBy(r => r.RegionId)
                    .ToArray();
                if (listedRegions.Length == 0) continue;
                if (listedRegions.Any(r => string.Equals(r.RegionId, assignedRegion.RegionId, StringComparison.Ordinal)))
                    continue;

                findings.Add(new ValidationFinding(ValidationSeverity.Error,
                    ParallelValidationCodes.InvalidMembership,
                    $"State '{state.Value}' is listed in region '{listedRegions[0].Name}' but its state metadata assigns it to region '{assignedRegion.Name}' under parallel composite '{compositeGroup.Key}'.",
                    $"state:{state.Value}",
                    "Keep region member lists and state-level region assignment consistent; prefer region-scoped declarations to avoid membership drift.",
                    compositeGroup.Key));
            }

            foreach (var child in definition.GetChildren(compositeGroup.Key))
                if (!all.Any(x => EqualityComparer<TState>.Default.Equals(x.State, child.Value)))
                    findings.Add(new ValidationFinding(ValidationSeverity.Error,
                        ParallelValidationCodes.InvalidMembership,
                        $"State '{child.Value}' is a direct child of parallel composite '{compositeGroup.Key}' but is not assigned to a region.",
                        $"state:{child.Value}",
                        "Assign the child state to one named region; region-scoped declarations assign membership automatically.",
                        compositeGroup.Key));
        }
    }
}