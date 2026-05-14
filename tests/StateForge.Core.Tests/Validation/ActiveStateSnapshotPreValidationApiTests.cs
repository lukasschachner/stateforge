using StateForge.Core.Tests.Execution;
using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Validation;

public sealed class ActiveStateSnapshotPreValidationApiTests
{
    [Fact]
    public void ValidateActiveStateSnapshotReturnsDiagnosticsWithoutCreatingRuntime()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateFlatDefinition();
        var externalSnapshot = ActiveStateSnapshot<SnapshotState>.SingleLeaf(SnapshotState.Other);

        var result = definition.ValidateActiveStateSnapshot(externalSnapshot);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Diagnostics);
    }

    [Fact]
    public void ValidateActiveStateSnapshotAcceptsValidExternalSnapshot()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateFlatDefinition();
        var externalSnapshot = ActiveStateSnapshot<SnapshotState>.SingleLeaf(SnapshotState.Idle);

        var result = definition.ValidateActiveStateSnapshot(externalSnapshot);

        Assert.True(result.IsValid);
    }
}
