using StateForge.Core.Introspection;

namespace StateForge.Core.Tests.Introspection;

public class GraphExportValidDefinitionTests
{
    [Fact]
    public void GraphExportPreservesNodeAndEdgeCountsInDeclarationOrder()
    {
        var definition = GraphExportTestData.CreateValidOrderDefinition();

        var export = definition.ExportGraph();

        Assert.True(export.Succeeded, export.FailureSummary);
        var graph = Assert.IsType<DefinitionGraph<OrderState, OrderEvent>>(export.Graph);
        Assert.Equal(new[] { OrderState.Created, OrderState.Paid, OrderState.Shipped, OrderState.Cancelled },
            graph.Nodes.Select(n => n.State));
        Assert.Equal(new[] { "state-000", "state-001", "state-002", "state-003" }, graph.Nodes.Select(n => n.Id));
        Assert.Equal(4, graph.Edges.Count);
        Assert.Equal(new[] { "transition-000", "transition-001", "transition-002", "transition-003" },
            graph.Edges.Select(e => e.Id));
        Assert.Equal(new[] { OrderState.Created, OrderState.Created, OrderState.Paid, OrderState.Paid },
            graph.Edges.Select(e => e.SourceState));
        Assert.Equal(new[] { OrderState.Paid, OrderState.Cancelled, OrderState.Shipped, OrderState.Cancelled },
            graph.Edges.Select(e => e.TargetState));
    }

    [Fact]
    public void GraphExportIsAvailableFromDefinitionIntrospection()
    {
        var definition = GraphExportTestData.CreateValidOrderDefinition();

        var export = definition.Introspect().ExportGraph();

        Assert.True(export.Succeeded, export.FailureSummary);
        Assert.NotNull(export.Graph);
    }
}