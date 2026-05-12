using Core.Tests.Execution;
using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Core.Validation;

namespace Core.Tests.Validation;

public sealed class ActiveStateSnapshotValidationCompatibilityTests
{
    [Fact]
    public void ValidationReportsUnsupportedSnapshotKind()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateFlatDefinition();
        var snapshot = new ActiveStateSnapshot<SnapshotState>((ActiveStateSnapshotKind)999,
            activeLeafState: SnapshotState.Idle);

        var result = definition.ValidateActiveStateSnapshot(snapshot);

        Assert.Contains(result.Diagnostics, d => d.Code == ActiveStateSnapshotValidationCodes.InvalidKind);
    }

    [Fact]
    public void ValidationReportsFingerprintMismatchWhenDefinitionDeclaresFingerprint()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateParallelDefinition("expected");
        var snapshot = definition.CreateRuntime(SnapshotState.Operational)
            .CaptureActiveStateSnapshot("actual");

        var result = definition.ValidateActiveStateSnapshot(snapshot);

        Assert.Contains(result.Diagnostics, d => d.Code == ActiveStateSnapshotValidationCodes.FingerprintMismatch);
    }

    [Fact]
    public void ValidationReportsNegativeSequence()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateFlatDefinition();
        var snapshot = ActiveStateSnapshot<SnapshotState>.SingleLeaf(SnapshotState.Idle, sequence: -1);

        var result = definition.ValidateActiveStateSnapshot(snapshot);

        Assert.Contains(result.Diagnostics, d => d.Code == ActiveStateSnapshotValidationCodes.SequenceInvalid);
    }
}
