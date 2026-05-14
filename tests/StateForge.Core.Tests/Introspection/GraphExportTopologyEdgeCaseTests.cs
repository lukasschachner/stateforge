using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Introspection;

public class GraphExportTopologyEdgeCaseTests
{
    [Fact]
    public void GraphExportPreservesNoTransitionMachines()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.State("Only");
            builder.State("Other");
        });

        var export = definition.ExportGraph();

        Assert.True(export.Succeeded, export.FailureSummary);
        Assert.Equal(new[] { "Only", "Other" }, export.Graph!.Nodes.Select(n => n.State));
        Assert.Empty(export.Graph.Edges);
    }

    [Fact]
    public void GraphExportPreservesSelfInternalMultipleEdgesAndCycles()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created)
                .On<Pay>().Self()
                .On<Ship>().Internal()
                .On<Cancel>().GoTo(OrderState.Paid);
            builder.State(OrderState.Paid)
                .On<Refund>().GoTo(OrderState.Created);
        });

        var export = definition.ExportGraph();

        Assert.True(export.Succeeded, export.FailureSummary);
        var graph = export.Graph!;
        Assert.Equal(4, graph.Edges.Count);
        Assert.Equal(2, graph.Edges.Count(e => e.SourceNodeId == "state-000" && e.TargetNodeId == "state-000"));
        Assert.Contains(graph.Edges,
            e => e.Kind == TransitionKind.Self && e.SourceState == OrderState.Created &&
                 e.TargetState == OrderState.Created);
        Assert.Contains(graph.Edges,
            e => e.Kind == TransitionKind.Internal && e.SourceState == OrderState.Created &&
                 e.TargetState == OrderState.Created);
        Assert.Contains(graph.Edges, e => e.SourceState == OrderState.Created && e.TargetState == OrderState.Paid);
        Assert.Contains(graph.Edges, e => e.SourceState == OrderState.Paid && e.TargetState == OrderState.Created);
    }
}