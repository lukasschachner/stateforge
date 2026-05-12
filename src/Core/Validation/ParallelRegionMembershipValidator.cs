using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Validation;

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

            foreach (var child in definition.GetChildren(compositeGroup.Key))
                if (!all.Any(x => EqualityComparer<TState>.Default.Equals(x.State, child.Value)))
                    findings.Add(new ValidationFinding(ValidationSeverity.Error,
                        ParallelValidationCodes.InvalidMembership,
                        $"State '{child.Value}' is a direct child of parallel composite '{compositeGroup.Key}' but is not assigned to a region.",
                        $"state:{child.Value}", "Assign the child state to one named region.", compositeGroup.Key));
        }
    }
}