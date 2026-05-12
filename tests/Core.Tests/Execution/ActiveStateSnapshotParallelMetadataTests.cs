namespace Core.Tests.Execution;

public sealed class ActiveStateSnapshotParallelMetadataTests
{
    [Fact]
    public async Task SnapshotCarriesSequenceAndDefinitionFingerprintThroughRestore()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateParallelDefinition("parallel-v1");
        var runtime = definition.CreateRuntime(SnapshotState.Operational);

        await runtime.ApplyAsync(SnapshotEvent.Pack);
        var snapshot = runtime.CaptureActiveStateSnapshot();

        Assert.Equal(1, snapshot.Sequence);
        Assert.Equal("parallel-v1", snapshot.DefinitionFingerprint);

        var restored = definition.CreateRuntime(snapshot);

        Assert.Equal(1, restored.CaptureActiveStateSnapshot().Sequence);
    }
}
