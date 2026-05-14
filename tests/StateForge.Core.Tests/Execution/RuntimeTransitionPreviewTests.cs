using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Execution;

public sealed class RuntimeTransitionPreviewTests
{
    [Fact]
    public async Task RuntimePreviewDoesNotMutateCurrentStateShapeOrHistory()
    {
        var runtime = TransitionPreviewTestDomain.Guarded().CreateRuntime(PreviewState.Draft);
        var beforeState = runtime.CurrentState;
        var beforeShape = runtime.ActiveStateShape;
        var beforeHistory = runtime.HistorySnapshots.ToArray();
        var beforeParallelHistory = runtime.ParallelHistorySnapshots.ToArray();

        var preview = await runtime.PreviewAsync(new PreviewSubmit());

        Assert.True(preview.IsPermitted);
        Assert.Equal(beforeState, runtime.CurrentState);
        Assert.Same(beforeShape, runtime.ActiveStateShape);
        Assert.Equal(beforeHistory, runtime.HistorySnapshots);
        Assert.Equal(beforeParallelHistory, runtime.ParallelHistorySnapshots);
    }

    [Fact]
    public async Task SerializedRuntimePreviewUsesCurrentShapeWithoutCommit()
    {
        var runtime = TransitionPreviewTestDomain.Guarded().CreateRuntime(PreviewState.Draft,
            ConcurrencyMode.Serialized);

        var preview = await runtime.PreviewAsync(new PreviewSubmit());

        Assert.True(preview.IsPermitted);
        Assert.Equal(PreviewState.Draft, runtime.CurrentState);
    }
}
