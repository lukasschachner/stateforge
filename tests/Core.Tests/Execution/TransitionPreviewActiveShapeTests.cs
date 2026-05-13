using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Execution;

public sealed class TransitionPreviewActiveShapeTests
{
    [Fact]
    public async Task PreviewPredictsHierarchicalLeafTarget()
    {
        var definition = TransitionPreviewTestDomain.Hierarchical();

        var preview = await definition.PreviewAsync(ActiveStateShape<PreviewState>.Single(PreviewState.ChildA),
            new PreviewSubmit());

        preview.AssertPermitted(PreviewState.ChildB);
        Assert.Equal(PreviewState.ChildB, preview.ExpectedActiveShape?.ActiveLeafState);
    }

    [Fact]
    public async Task PreviewPredictsParallelRegionalShape()
    {
        var definition = TransitionPreviewTestDomain.Parallel();
        var runtime = definition.CreateRuntime(PreviewState.Parallel);

        var preview = await runtime.PreviewAsync(new PreviewSubmit());

        Assert.True(preview.IsPermitted);
        Assert.NotNull(preview.ExpectedActiveShape);
        Assert.True(preview.ExpectedActiveShape!.IsParallel);
        Assert.All(preview.ExpectedActiveShape.ActiveRegions, region => Assert.True(region.IsTerminal));
    }
}
