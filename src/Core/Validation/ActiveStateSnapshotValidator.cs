using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;

namespace StateMachineLibrary.Core.Validation;

/// <summary>Validates externally supplied active-state snapshots before runtime construction.</summary>
public static class ActiveStateSnapshotValidator
{
    public static ActiveStateSnapshotValidationResult<TState> Validate<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        ActiveStateSnapshot<TState> snapshot)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(snapshot);

        var diagnostics = new List<ActiveStateSnapshotValidationDiagnostic<TState>>();
        var definitionValidation = definition.Validate();
        if (!definitionValidation.IsValid)
            diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                ActiveStateSnapshotValidationCodes.DefinitionInvalid,
                "Machine definition has validation errors and cannot restore active-state snapshots.",
                snapshot.Kind,
                snapshot.Sequence));

        ValidateKindAndMetadata(definition, snapshot, diagnostics);
        if (!Enum.IsDefined(snapshot.Kind))
            return new ActiveStateSnapshotValidationResult<TState>(diagnostics);

        switch (snapshot.Kind)
        {
            case ActiveStateSnapshotKind.SingleLeaf:
                ValidateSingleLeaf(definition, snapshot, diagnostics, requirePath: false);
                if (snapshot.RegionSnapshots.Count > 0)
                    diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                        ActiveStateSnapshotValidationCodes.RegionMetadataNotAllowed,
                        "Single-leaf snapshots must not contain parallel region metadata.",
                        snapshot.Kind,
                        snapshot.Sequence));
                break;
            case ActiveStateSnapshotKind.Hierarchical:
                ValidateSingleLeaf(definition, snapshot, diagnostics, requirePath: true);
                if (snapshot.RegionSnapshots.Count > 0)
                    diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                        ActiveStateSnapshotValidationCodes.RegionMetadataNotAllowed,
                        "Hierarchical snapshots must not contain parallel region metadata.",
                        snapshot.Kind,
                        snapshot.Sequence));
                break;
            case ActiveStateSnapshotKind.Parallel:
                ValidateParallel(definition, snapshot, diagnostics);
                break;
        }

        return new ActiveStateSnapshotValidationResult<TState>(diagnostics);
    }

    private static void ValidateKindAndMetadata<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        ActiveStateSnapshot<TState> snapshot,
        List<ActiveStateSnapshotValidationDiagnostic<TState>> diagnostics)
    {
        if (!Enum.IsDefined(snapshot.Kind))
            diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                ActiveStateSnapshotValidationCodes.InvalidKind,
                $"Snapshot kind '{snapshot.Kind}' is not supported.",
                snapshot.Kind,
                snapshot.Sequence));

        if (snapshot.Sequence < 0)
            diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                ActiveStateSnapshotValidationCodes.SequenceInvalid,
                "Snapshot sequence must be zero or greater.",
                snapshot.Kind,
                snapshot.Sequence));

        if (snapshot.DefinitionFingerprint is null) return;

        if (!definition.Metadata.TryGetValue(StateMachineMetadataKeys.DefinitionFingerprint, out var value) ||
            value is null)
            return;

        var expected = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
        if (!string.Equals(expected, snapshot.DefinitionFingerprint, StringComparison.Ordinal))
            diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                ActiveStateSnapshotValidationCodes.FingerprintMismatch,
                "Snapshot definition fingerprint does not match the target definition.",
                snapshot.Kind,
                snapshot.Sequence));
    }

    private static void ValidateSingleLeaf<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        ActiveStateSnapshot<TState> snapshot,
        List<ActiveStateSnapshotValidationDiagnostic<TState>> diagnostics,
        bool requirePath)
    {
        if (snapshot.ActiveLeafState is null)
        {
            diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                ActiveStateSnapshotValidationCodes.MissingActiveLeafState,
                "Snapshot active leaf state is required.",
                snapshot.Kind,
                snapshot.Sequence));
            return;
        }

        ValidateKnownState(definition, snapshot, snapshot.ActiveLeafState, diagnostics);
        if (requirePath)
        {
            if (snapshot.ActivePath is null)
            {
                diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                    ActiveStateSnapshotValidationCodes.MissingActivePath,
                    "Hierarchical snapshots require an ordered active path.",
                    snapshot.Kind,
                    snapshot.Sequence,
                    snapshot.ActiveLeafState));
                return;
            }

            ValidatePath(definition, snapshot, snapshot.ActiveLeafState, snapshot.ActivePath, diagnostics);
            return;
        }

        if (snapshot.ActivePath is not null)
            ValidatePath(definition, snapshot, snapshot.ActiveLeafState, snapshot.ActivePath, diagnostics);
    }

    private static void ValidateParallel<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        ActiveStateSnapshot<TState> snapshot,
        List<ActiveStateSnapshotValidationDiagnostic<TState>> diagnostics)
    {
        if (snapshot.OwningCompositeState is null)
        {
            diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                ActiveStateSnapshotValidationCodes.MissingOwningCompositeState,
                "Parallel snapshots require an owning composite state.",
                snapshot.Kind,
                snapshot.Sequence));
            return;
        }

        var owner = snapshot.OwningCompositeState;
        ValidateKnownState(definition, snapshot, owner, diagnostics);
        if (!definition.IsParallelComposite(owner))
            diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                ActiveStateSnapshotValidationCodes.InvalidRegionOwner,
                "Snapshot owning state is not a parallel composite in the target definition.",
                snapshot.Kind,
                snapshot.Sequence,
                owner,
                owner));

        var declaredRegions = definition.GetParallelRegions(owner).OrderBy(region => region.Order).ToArray();
        if (declaredRegions.Length == 0)
        {
            foreach (var region in snapshot.RegionSnapshots)
                diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                    ActiveStateSnapshotValidationCodes.UnknownRegion,
                    "Snapshot region does not belong to the owning composite.",
                    snapshot.Kind,
                    snapshot.Sequence,
                    region.ActiveLeafState,
                    owner,
                    region.RegionId,
                    region.RegionName));
            return;
        }

        var regionById = declaredRegions.ToDictionary(region => region.RegionId, StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var group in snapshot.RegionSnapshots.GroupBy(region => region.RegionId, StringComparer.Ordinal))
            if (group.Count() > 1)
                diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                    ActiveStateSnapshotValidationCodes.DuplicateRegion,
                    "Snapshot contains duplicate entries for a parallel region.",
                    snapshot.Kind,
                    snapshot.Sequence,
                    OwningCompositeState: owner,
                    RegionId: group.Key));

        foreach (var regionSnapshot in snapshot.RegionSnapshots)
        {
            seen.Add(regionSnapshot.RegionId);
            if (!regionById.TryGetValue(regionSnapshot.RegionId, out var regionDefinition))
            {
                diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                    ActiveStateSnapshotValidationCodes.UnknownRegion,
                    "Snapshot region id is not declared by the owning composite.",
                    snapshot.Kind,
                    snapshot.Sequence,
                    regionSnapshot.ActiveLeafState,
                    owner,
                    regionSnapshot.RegionId,
                    regionSnapshot.RegionName));
                continue;
            }

            if (!string.IsNullOrEmpty(regionSnapshot.RegionName) &&
                !string.Equals(regionDefinition.Name, regionSnapshot.RegionName, StringComparison.Ordinal))
                diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                    ActiveStateSnapshotValidationCodes.RegionNameMismatch,
                    "Snapshot region name does not match the declared region name.",
                    snapshot.Kind,
                    snapshot.Sequence,
                    regionSnapshot.ActiveLeafState,
                    owner,
                    regionSnapshot.RegionId,
                    regionSnapshot.RegionName));

            ValidateRegionLeaf(definition, snapshot, regionDefinition, regionSnapshot, diagnostics);
            ValidatePath(definition, snapshot, regionSnapshot.ActiveLeafState, regionSnapshot.ActivePath, diagnostics,
                regionSnapshot.RegionId, regionSnapshot.RegionName, owner);

            var expectedTerminal = regionDefinition.TerminalStates.Contains(regionSnapshot.ActiveLeafState,
                EqualityComparer<TState>.Default);
            if (regionSnapshot.IsTerminal != expectedTerminal)
                diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                    ActiveStateSnapshotValidationCodes.InvalidTerminalFlag,
                    "Snapshot region terminal flag does not match the declared terminal-state metadata.",
                    snapshot.Kind,
                    snapshot.Sequence,
                    regionSnapshot.ActiveLeafState,
                    owner,
                    regionSnapshot.RegionId,
                    regionSnapshot.RegionName));
        }

        foreach (var missing in declaredRegions.Where(region => !seen.Contains(region.RegionId)))
            diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                ActiveStateSnapshotValidationCodes.MissingRegion,
                "Parallel snapshots must contain exactly one entry for each declared region.",
                snapshot.Kind,
                snapshot.Sequence,
                OwningCompositeState: owner,
                RegionId: missing.RegionId,
                RegionName: missing.Name));
    }

    private static void ValidateRegionLeaf<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        ActiveStateSnapshot<TState> snapshot,
        ParallelRegionDefinition<TState> regionDefinition,
        ActiveRegionSnapshot<TState> regionSnapshot,
        List<ActiveStateSnapshotValidationDiagnostic<TState>> diagnostics)
    {
        ValidateKnownState(definition, snapshot, regionSnapshot.ActiveLeafState, diagnostics, regionSnapshot.RegionId,
            regionSnapshot.RegionName, regionDefinition.OwnerCompositeState);

        var belongsToRegion = regionDefinition.MemberStates.Contains(regionSnapshot.ActiveLeafState,
                                  EqualityComparer<TState>.Default) ||
                              regionDefinition.TerminalStates.Contains(regionSnapshot.ActiveLeafState,
                                  EqualityComparer<TState>.Default);
        if (!belongsToRegion && definition.TryGetRegionMembership(regionSnapshot.ActiveLeafState, out var membership))
            belongsToRegion = string.Equals(membership.RegionId, regionDefinition.RegionId, StringComparison.Ordinal);

        if (!belongsToRegion)
            diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                ActiveStateSnapshotValidationCodes.InvalidRegionState,
                "Snapshot active leaf state does not belong to the declared parallel region.",
                snapshot.Kind,
                snapshot.Sequence,
                regionSnapshot.ActiveLeafState,
                regionDefinition.OwnerCompositeState,
                regionSnapshot.RegionId,
                regionSnapshot.RegionName));
    }

    private static void ValidateKnownState<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        ActiveStateSnapshot<TState> snapshot,
        TState state,
        List<ActiveStateSnapshotValidationDiagnostic<TState>> diagnostics,
        string? regionId = null,
        string? regionName = null,
        TState? owner = default)
    {
        if (definition.ContainsState(state)) return;

        diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
            ActiveStateSnapshotValidationCodes.UnknownState,
            "Snapshot references a state that is not declared by the target definition.",
            snapshot.Kind,
            snapshot.Sequence,
            state,
            owner,
            regionId,
            regionName));
    }

    private static void ValidatePath<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        ActiveStateSnapshot<TState> snapshot,
        TState activeLeafState,
        ActiveStatePath<TState>? activePath,
        List<ActiveStateSnapshotValidationDiagnostic<TState>> diagnostics,
        string? regionId = null,
        string? regionName = null,
        TState? owner = default)
    {
        if (activePath is null)
        {
            diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                ActiveStateSnapshotValidationCodes.MissingActivePath,
                "Snapshot active path is required.",
                snapshot.Kind,
                snapshot.Sequence,
                activeLeafState,
                owner,
                regionId,
                regionName));
            return;
        }

        foreach (var segment in activePath.StatesRootToLeaf)
            if (!definition.ContainsState(segment))
                diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                    ActiveStateSnapshotValidationCodes.UnknownState,
                    "Snapshot active path references an unknown state.",
                    snapshot.Kind,
                    snapshot.Sequence,
                    segment,
                    owner,
                    regionId,
                    regionName,
                    segment));

        var comparer = EqualityComparer<TState>.Default;
        if (!comparer.Equals(activePath.ActiveLeafState, activeLeafState))
            diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                ActiveStateSnapshotValidationCodes.InvalidPath,
                "Snapshot active path must terminate at the active leaf state.",
                snapshot.Kind,
                snapshot.Sequence,
                activeLeafState,
                owner,
                regionId,
                regionName,
                activePath.ActiveLeafState));

        if (!definition.ContainsState(activeLeafState)) return;

        var expected = definition.GetActiveStatePath(activeLeafState).StatesRootToLeaf;
        if (!expected.SequenceEqual(activePath.StatesRootToLeaf, comparer))
            diagnostics.Add(new ActiveStateSnapshotValidationDiagnostic<TState>(
                ActiveStateSnapshotValidationCodes.InvalidPath,
                "Snapshot active path does not match the target definition ancestry.",
                snapshot.Kind,
                snapshot.Sequence,
                activeLeafState,
                owner,
                regionId,
                regionName));
    }
}
