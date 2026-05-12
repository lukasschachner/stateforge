using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Introspection;

namespace Core.Tests.Introspection;

public class GraphExportEdgeDetailsTests
{
    [Fact]
    public void GraphExportIncludesEdgeLabelsEventsKindsConditionSummariesAndMetadata()
    {
        var definition = GraphExportTestData.CreateValidOrderDefinition();

        var graph = Assert.IsType<DefinitionGraph<OrderState, OrderEvent>>(definition.ExportGraph().Graph);

        var pay = graph.Edges[0];
        Assert.Equal("Pay", pay.Label);
        Assert.Equal("type:Core.Tests.Pay", pay.Event.Identity);
        Assert.Equal("Pay", pay.Event.DisplayName);
        Assert.Equal("Type", pay.Event.Category);
        Assert.Equal(TransitionKind.External, pay.Kind);
        Assert.Equal(GraphConditionSummaryKind.All, pay.Conditions.Kind);
        var condition = Assert.Single(pay.Conditions.Conditions);
        Assert.Equal(0, condition.Position);
        Assert.Equal("payment amount is positive", condition.DisplayName);
        Assert.Equal("Payment must be captured before shipping", pay.Metadata["businessRule"]);

        var cancel = graph.Edges[1];
        Assert.Equal(GraphConditionSummaryKind.None, cancel.Conditions.Kind);
        Assert.Empty(cancel.Conditions.Conditions);
        Assert.Equal("No conditions", cancel.Conditions.DisplayText);
    }

    [Fact]
    public void GraphExportDistinguishesValueEventDescriptors()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.State("A").On("go").GoTo("B");
            builder.State("B").Terminal();
        });

        var graph = Assert.IsType<DefinitionGraph<string, string>>(definition.ExportGraph().Graph);
        var edge = Assert.Single(graph.Edges);
        Assert.Equal("value:go", edge.Event.Identity);
        Assert.Equal("go", edge.Event.DisplayName);
        Assert.Equal("Value", edge.Event.Category);
    }
}