using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Validation;

internal static class ParallelRegionStructureValidator
{
    public static void Validate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        ICollection<ValidationFinding> findings)
    {
        foreach (var composite in definition.States.Where(s => s.Hierarchy.IsParallelComposite))
        {
            var regions = definition.GetParallelRegions(composite.Value);
            if (regions.Count == 0)
                findings.Add(new ValidationFinding(ValidationSeverity.Error, ParallelValidationCodes.ZeroRegions,
                    $"Parallel composite '{composite.Value}' must declare at least one region.",
                    $"state:{composite.Value}", "Declare one or more named regions.", composite.Value));
        }

        foreach (var group in definition.ParallelRegions.GroupBy(r => r.OwnerCompositeState,
                     EqualityComparer<TState>.Default))
        {
            foreach (var region in group)
            {
                if (string.IsNullOrWhiteSpace(region.Name))
                    findings.Add(new ValidationFinding(ValidationSeverity.Error,
                        ParallelValidationCodes.BlankRegionName,
                        $"Parallel region under '{region.OwnerCompositeState}' must have a non-blank name.",
                        $"region:{region.RegionId}", "Choose a non-empty region name.", region.OwnerCompositeState,
                        region.RegionId, region.Name));
                if (!region.HasInitialState ||
                    !region.MemberStates.Contains(region.InitialState!, EqualityComparer<TState>.Default))
                    findings.Add(new ValidationFinding(ValidationSeverity.Error, ParallelValidationCodes.MissingInitial,
                        $"Parallel region '{region.Name}' must declare an initial state that belongs to the region.",
                        $"region:{region.RegionId}", "Set the region initial state to one of its member states.",
                        region.OwnerCompositeState, region.RegionId, region.Name));
            }

            foreach (var duplicate in
                     group.GroupBy(r => (r.Name ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase)
                         .Where(g => !string.IsNullOrWhiteSpace(g.Key) && g.Count() > 1))
                findings.Add(new ValidationFinding(ValidationSeverity.Error,
                    ParallelValidationCodes.DuplicateRegionName,
                    $"Parallel composite '{group.Key}' has duplicate region name '{duplicate.Key}'.",
                    $"state:{group.Key}:region:{duplicate.Key}", "Use unique sibling region names.", group.Key, null,
                    duplicate.Key));
        }
    }
}