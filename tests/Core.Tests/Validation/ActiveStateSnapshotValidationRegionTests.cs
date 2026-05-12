using Core.Tests.Execution;
using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Core.Validation;

namespace Core.Tests.Validation;

public sealed class ActiveStateSnapshotValidationRegionTests
{
    [Fact]
    public void ValidationReportsUnknownMissingAndDuplicateParallelRegions()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateParallelDefinition();
        var valid = definition.CreateRuntime(SnapshotState.Operational).CaptureActiveStateSnapshot();
        var first = valid.RegionSnapshots[0];
        var snapshot = ActiveStateSnapshot<SnapshotState>.Parallel(
            SnapshotState.Operational,
            [first, first, first with { RegionId = "unknown" }]);

        var result = definition.ValidateActiveStateSnapshot(snapshot);

        Assert.Contains(result.Diagnostics, d => d.Code == ActiveStateSnapshotValidationCodes.DuplicateRegion);
        Assert.Contains(result.Diagnostics, d => d.Code == ActiveStateSnapshotValidationCodes.UnknownRegion);
        Assert.Contains(result.Diagnostics, d => d.Code == ActiveStateSnapshotValidationCodes.MissingRegion);
    }

    [Fact]
    public void ValidationReportsRegionNameMismatchAndWrongTerminalFlag()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateParallelDefinition();
        var snapshot = definition.CreateRuntime(SnapshotState.Operational).CaptureActiveStateSnapshot();
        var mutatedRegion = snapshot.RegionSnapshots[0] with { RegionName = "Wrong", IsTerminal = true };
        var mutated = ActiveStateSnapshot<SnapshotState>.Parallel(
            SnapshotState.Operational,
            [mutatedRegion, snapshot.RegionSnapshots[1]]);

        var result = definition.ValidateActiveStateSnapshot(mutated);

        Assert.Contains(result.Diagnostics, d => d.Code == ActiveStateSnapshotValidationCodes.RegionNameMismatch);
        Assert.Contains(result.Diagnostics, d => d.Code == ActiveStateSnapshotValidationCodes.InvalidTerminalFlag);
    }
}
