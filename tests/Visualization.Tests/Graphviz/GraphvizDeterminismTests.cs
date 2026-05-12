using StateMachineLibrary.Visualization.Graphviz.Rendering;
using Visualization.Tests.TestSupport;

namespace Visualization.Tests.Graphviz;

public sealed class GraphvizDeterminismTests
{
    [Fact]
    public void GraphvizRendering_IsDeterministicAcrossRepeatedCalls()
    {
        var graph = GraphRenderingFixtureFactory.CreateOrderFlowGraph();
        var first = GraphvizDotRenderer.Render(graph);
        var second = GraphvizDotRenderer.Render(graph);

        Assert.Equal(TextNormalization.NormalizeForSnapshot(first), TextNormalization.NormalizeForSnapshot(second));
    }

    [Fact]
    public void GraphvizRendering_MatchesApprovedSnapshot()
    {
        var graph = GraphRenderingFixtureFactory.CreateOrderFlowGraph();
        var rendered = GraphvizDotRenderer.Render(graph);
        DiagramSnapshotAssert.MatchesApproved("graphviz.approved.txt", rendered);
    }
}