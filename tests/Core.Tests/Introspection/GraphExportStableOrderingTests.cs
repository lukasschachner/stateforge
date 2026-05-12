namespace Core.Tests.Introspection;

public class GraphExportStableOrderingTests
{
    [Fact]
    public void GraphExportUsesStableOrderingForEquivalentDefinitions()
    {
        var first = GraphExportTestData.CreateValidOrderDefinition().ExportGraph().Graph!;
        var second = GraphExportTestData.CreateValidOrderDefinition().ExportGraph().Graph!;

        Assert.Equal(first.Nodes.Select(n => n.Id), second.Nodes.Select(n => n.Id));
        Assert.Equal(first.Nodes.Select(n => n.State), second.Nodes.Select(n => n.State));
        Assert.Equal(first.Edges.Select(e => e.Id), second.Edges.Select(e => e.Id));
        Assert.Equal(first.Edges.Select(e => e.SourceNodeId), second.Edges.Select(e => e.SourceNodeId));
        Assert.Equal(first.Edges.Select(e => e.TargetNodeId), second.Edges.Select(e => e.TargetNodeId));
        Assert.Equal(first.Edges.Select(e => e.Event.Identity), second.Edges.Select(e => e.Event.Identity));
    }
}