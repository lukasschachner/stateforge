namespace StateForge.Core.Tests.Introspection;

public class GraphExportAdapterConsumptionTests
{
    [Fact]
    public void GraphExportCanFeedAdapterUsingOnlyGraphData()
    {
        var definition = GraphExportTestData.CreateValidOrderDefinition();
        var graph = definition.ExportGraph().Graph!;

        var adapterLines = graph.Edges
            .Select(edge =>
                $"{graph.Nodes.Single(n => n.Id == edge.SourceNodeId).Label}|{edge.Event.DisplayName}|{edge.Kind}|{edge.Conditions.Kind}|{graph.Nodes.Single(n => n.Id == edge.TargetNodeId).Label}")
            .ToArray();

        Assert.Contains("Created|Pay|External|All|Paid", adapterLines);
        Assert.Contains("Paid|Ship|External|None|Shipped", adapterLines);
        Assert.Equal("Order lifecycle", graph.Metadata["title"]);
    }
}