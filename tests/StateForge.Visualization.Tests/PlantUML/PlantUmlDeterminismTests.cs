using StateForge.Visualization.PlantUML.Rendering;
using StateForge.Visualization.Tests.TestSupport;

namespace StateForge.Visualization.Tests.PlantUML;

public sealed class PlantUmlDeterminismTests
{
    [Fact]
    public void PlantUmlRendering_IsDeterministicAcrossRepeatedCalls()
    {
        var graph = GraphRenderingFixtureFactory.CreateOrderFlowGraph();
        var first = PlantUmlGraphRenderer.Render(graph);
        var second = PlantUmlGraphRenderer.Render(graph);

        Assert.Equal(TextNormalization.NormalizeForSnapshot(first), TextNormalization.NormalizeForSnapshot(second));
    }

    [Fact]
    public void PlantUmlRendering_MatchesApprovedSnapshot()
    {
        var graph = GraphRenderingFixtureFactory.CreateOrderFlowGraph();
        var rendered = PlantUmlGraphRenderer.Render(graph);
        DiagramSnapshotAssert.MatchesApproved("plantuml.approved.txt", rendered);
    }
}