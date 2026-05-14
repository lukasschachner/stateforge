using StateForge.Core.Introspection;

namespace StateForge.Core.Tests.Introspection;

public class GraphExportNodeMetadataTests
{
    [Fact]
    public void GraphExportPreservesTerminalFlagsAndNodeMetadata()
    {
        var definition = GraphExportTestData.CreateValidOrderDefinition();

        var graph = Assert.IsType<DefinitionGraph<OrderState, OrderEvent>>(definition.ExportGraph().Graph);

        Assert.False(graph.Nodes.Single(n => n.State == OrderState.Created).IsTerminal);
        Assert.False(graph.Nodes.Single(n => n.State == OrderState.Paid).IsTerminal);
        Assert.True(graph.Nodes.Single(n => n.State == OrderState.Shipped).IsTerminal);
        Assert.True(graph.Nodes.Single(n => n.State == OrderState.Cancelled).IsTerminal);
        Assert.Equal("Order was created",
            graph.Nodes.Single(n => n.State == OrderState.Created).Metadata["description"]);
    }

    [Fact]
    public void GraphExportPreservesGraphMetadataAndExposesExplicitEmptyMetadata()
    {
        var definition = GraphExportTestData.CreateValidOrderDefinition();

        var graph = Assert.IsType<DefinitionGraph<OrderState, OrderEvent>>(definition.ExportGraph().Graph);

        Assert.Equal("Order lifecycle", graph.Metadata["title"]);
        Assert.Equal("Order lifecycle", graph.Label);
        Assert.NotNull(graph.Nodes.Single(n => n.State == OrderState.Paid).Metadata);
        Assert.Empty(graph.Nodes.Single(n => n.State == OrderState.Paid).Metadata);
        Assert.NotNull(graph.Edges[1].Metadata);
        Assert.Empty(graph.Edges[1].Metadata);
        Assert.NotNull(graph.Edges[1].Event.Metadata);
        Assert.Empty(graph.Edges[1].Event.Metadata);
    }

    [Fact]
    public void GraphExportCollectionsAreReadOnlyAfterExport()
    {
        var definition = GraphExportTestData.CreateValidOrderDefinition();

        var graph = Assert.IsType<DefinitionGraph<OrderState, OrderEvent>>(definition.ExportGraph().Graph);
        var nodes = Assert.IsAssignableFrom<IList<GraphNode<OrderState>>>(graph.Nodes);
        var edges = Assert.IsAssignableFrom<IList<GraphEdge<OrderState, OrderEvent>>>(graph.Edges);
        var conditions = Assert.IsAssignableFrom<IList<GraphConditionDescriptor>>(graph.Edges[0].Conditions.Conditions);

        Assert.Throws<NotSupportedException>(() => nodes[0] = graph.Nodes[0]);
        Assert.Throws<NotSupportedException>(() => edges[0] = graph.Edges[0]);
        Assert.Throws<NotSupportedException>(() => conditions[0] = graph.Edges[0].Conditions.Conditions[0]);
    }
}