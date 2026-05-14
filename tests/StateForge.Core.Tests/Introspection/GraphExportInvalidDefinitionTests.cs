using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Introspection;

public class GraphExportInvalidDefinitionTests
{
    [Fact]
    public void GraphExportRefusesInvalidTargetDefinitions()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>().GoTo(OrderState.Paid);
        });

        var export = definition.ExportGraph();

        Assert.False(export.Succeeded);
        Assert.Null(export.Graph);
        Assert.Contains(export.Validation.Errors, f => f.Code == "TRANSITION002");
        Assert.Contains("validation produced", export.FailureSummary);
    }

    [Fact]
    public void GraphExportRefusesDuplicateTransitions()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created)
                .On<Pay>().GoTo(OrderState.Paid)
                .On<Pay>().GoTo(OrderState.Cancelled);
            builder.State(OrderState.Paid);
            builder.State(OrderState.Cancelled);
        });

        var export = definition.ExportGraph();

        Assert.False(export.Succeeded);
        Assert.Null(export.Graph);
        Assert.Contains(export.Validation.Errors, f => f.Code == "TRANSITION003");
    }
}