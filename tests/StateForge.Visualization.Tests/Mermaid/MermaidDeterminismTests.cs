using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.Tests.TestSupport;

namespace StateForge.Visualization.Tests.Mermaid;

public sealed class MermaidDeterminismTests
{
    [Fact]
    public void MermaidRendering_IsDeterministicAcrossRepeatedCalls()
    {
        var graph = GraphRenderingFixtureFactory.CreateOrderFlowGraph();
        var first = MermaidGraphRenderer.Render(graph);
        var second = MermaidGraphRenderer.Render(graph);

        Assert.Equal(TextNormalization.NormalizeForSnapshot(first), TextNormalization.NormalizeForSnapshot(second));
    }

    [Fact]
    public void MermaidRendering_MatchesApprovedSnapshot()
    {
        var graph = GraphRenderingFixtureFactory.CreateOrderFlowGraph();
        var rendered = MermaidGraphRenderer.Render(graph);
        DiagramSnapshotAssert.MatchesApproved("mermaid.approved.txt", rendered);
    }
}