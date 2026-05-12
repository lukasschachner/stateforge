using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;

namespace StateMachineLibrary.Core.Validation;

internal static class ParallelRegionHistoryValidator
{
    public static void Validate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        ICollection<ValidationFinding> findings)
    {
        foreach (var state in definition.HistoryEnabledStates.Where(s =>
                     s.Hierarchy.IsParallelComposite || definition.IsParallelComposite(s.Value)))
            ValidateParallelHistoryConfiguration(definition, state, findings);
    }

    public static ValidationResult ValidateSuppliedSnapshot<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition, ParallelHistorySnapshot<TState> snapshot)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(snapshot);

        var findings = new List<ValidationFinding>();
        if (!definition.ContainsState(snapshot.CompositeState))
        {
            findings.Add(new ValidationFinding(ValidationSeverity.Error, ParallelValidationCodes.SuppliedHistoryInvalid,
                $"Supplied parallel history references unknown composite state '{snapshot.CompositeState}'.",
                $"state:{snapshot.CompositeState}", "Supply history for a composite state in the current definition.",
                snapshot.CompositeState));
            return new ValidationResult(findings);
        }

        if (!definition.IsParallelComposite(snapshot.CompositeState) ||
            !definition.TryGetHistoryDefinition(snapshot.CompositeState, out var historyState))
        {
            findings.Add(new ValidationFinding(ValidationSeverity.Error,
                ParallelValidationCodes.InvalidHistoryConfiguration,
                $"Supplied parallel history composite '{snapshot.CompositeState}' is not a history-enabled parallel composite.",
                $"state:{snapshot.CompositeState}",
                "Supply history only for a parallel composite with direct history enabled.", snapshot.CompositeState));
            return new ValidationResult(findings);
        }

        if (historyState.HistoryMode != snapshot.HistoryMode)
            findings.Add(new ValidationFinding(ValidationSeverity.Error, ParallelValidationCodes.SuppliedHistoryInvalid,
                $"Supplied parallel history mode '{snapshot.HistoryMode}' does not match definition mode '{historyState.HistoryMode}'.",
                $"state:{snapshot.CompositeState}:history:mode",
                "Use a snapshot captured from the same history mode as the current definition.",
                snapshot.CompositeState));

        var knownRegions = definition.GetParallelRegions(snapshot.CompositeState)
            .ToDictionary(region => region.RegionId, StringComparer.Ordinal);
        foreach (var duplicate in snapshot.RegionEntries.GroupBy(entry => entry.RegionId, StringComparer.Ordinal)
                     .Where(group => group.Count() > 1))
        {
            var first = duplicate.First();
            findings.Add(new ValidationFinding(ValidationSeverity.Error, ParallelValidationCodes.DuplicateRegionHistory,
                $"Supplied parallel history has duplicate entries for region '{first.RegionName}' ({first.RegionId}).",
                $"region:{first.RegionId}", "Supply at most one history entry per owned region.",
                snapshot.CompositeState, first.RegionId, first.RegionName));
        }

        foreach (var entry in snapshot.RegionEntries)
        {
            if (!knownRegions.TryGetValue(entry.RegionId, out var region))
            {
                findings.Add(new ValidationFinding(ValidationSeverity.Error,
                    ParallelValidationCodes.UnknownRegionHistory,
                    $"Supplied parallel history references unknown or non-owned region '{entry.RegionId}'.",
                    $"region:{entry.RegionId}", "Supply entries only for regions owned by the composite.",
                    snapshot.CompositeState, entry.RegionId, entry.RegionName));
                continue;
            }

            if (!definition.ContainsState(entry.LastActiveLeafState))
            {
                findings.Add(new ValidationFinding(ValidationSeverity.Error,
                    ParallelValidationCodes.UnknownStateHistory,
                    $"Supplied parallel history for region '{region.Name}' references unknown state '{entry.LastActiveLeafState}'.",
                    $"state:{entry.LastActiveLeafState}", "Use states from the current definition.",
                    snapshot.CompositeState, region.RegionId, region.Name, TargetState: entry.LastActiveLeafState));
                continue;
            }

            if (!PathMatchesLeafAndRegion(definition, region, entry))
                findings.Add(new ValidationFinding(ValidationSeverity.Error, ParallelValidationCodes.InvalidRestorePath,
                    $"Supplied parallel history path for region '{region.Name}' is not a valid path to '{entry.LastActiveLeafState}' within that region.",
                    $"region:{region.RegionId}:path",
                    "Use a path that starts at the owning composite, stays within the region, and ends at the recorded leaf.",
                    snapshot.CompositeState, region.RegionId, region.Name, TargetState: entry.LastActiveLeafState));
        }

        return new ValidationResult(findings);
    }

    private static bool PathMatchesLeafAndRegion<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        ParallelRegionDefinition<TState> region, ParallelRegionHistoryEntry<TState> entry)
    {
        var comparer = EqualityComparer<TState>.Default;
        var suppliedPath = entry.LastActivePath.StatesRootToLeaf;
        if (suppliedPath.Count == 0 || !comparer.Equals(suppliedPath[^1], entry.LastActiveLeafState) ||
            !suppliedPath.Contains(region.OwnerCompositeState, comparer)) return false;

        var actualPath = definition.GetActiveStatePath(entry.LastActiveLeafState).StatesRootToLeaf;
        if (!actualPath.SequenceEqual(suppliedPath, comparer)) return false;

        return definition.TryGetRegionMembership(entry.LastActiveLeafState, out var membership) &&
               string.Equals(membership.RegionId, region.RegionId, StringComparison.Ordinal);
    }

    private static void ValidateParallelHistoryConfiguration<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition, StateDefinition<TState> state,
        ICollection<ValidationFinding> findings)
    {
        var regions = definition.GetParallelRegions(state.Value);
        if (regions.Count == 0)
        {
            findings.Add(new ValidationFinding(
                ValidationSeverity.Error,
                ParallelValidationCodes.InvalidHistoryConfiguration,
                $"Parallel history on '{state.Value}' requires at least one valid owned region.",
                $"state:{state.Value}",
                "Declare parallel regions with initial states or remove direct history from the composite.",
                state.Value));
            return;
        }

        foreach (var region in regions)
        {
            if (!region.HasInitialState || !definition.ContainsState(region.InitialState!))
            {
                findings.Add(new ValidationFinding(
                    ValidationSeverity.Error,
                    ParallelValidationCodes.MissingFallback,
                    $"Parallel history on '{state.Value}' requires region '{region.Name}' to have a valid initial fallback state.",
                    $"region:{region.RegionId}",
                    "Configure an initial state for every parallel region.",
                    state.Value,
                    region.RegionId,
                    region.Name));
                continue;
            }

            var fallbackLeaf = InitialChildResolver.ResolveTargetLeaf(definition, region.InitialState!);
            if (!definition.TryGetRegionMembership(fallbackLeaf, out var membership) ||
                !string.Equals(membership.RegionId, region.RegionId, StringComparison.Ordinal))
                findings.Add(new ValidationFinding(
                    ValidationSeverity.Error,
                    ParallelValidationCodes.InvalidFallback,
                    $"Parallel history fallback state '{region.InitialState}' for region '{region.Name}' does not resolve inside that region.",
                    $"region:{region.RegionId}",
                    "Use a fallback initial state owned by the region.",
                    state.Value,
                    region.RegionId,
                    region.Name,
                    TargetState: region.InitialState));
        }
    }
}