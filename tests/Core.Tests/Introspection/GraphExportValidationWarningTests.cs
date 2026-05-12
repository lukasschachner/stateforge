using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Introspection;

public class GraphExportValidationWarningTests
{
    [Fact]
    public void GraphExportSucceedsAndPreservesWarningsWhenDefinitionHasNoErrors()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created);
            builder.State(OrderState.Paid);
        });

        var export = definition.ExportGraph();

        Assert.True(export.Succeeded, export.FailureSummary);
        Assert.NotNull(export.Graph);
        Assert.Null(export.FailureSummary);
        Assert.Contains(export.Validation.Warnings, f => f.Code == "STATE003");
        Assert.Contains(export.Graph.Validation.Warnings, f => f.Code == "STATE003");
    }

    [Fact]
    public void GraphExportDoesNotRequireRuntimeCreationOrInitialState()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created);
        });

        var export = definition.ExportGraph();

        Assert.True(export.Succeeded, export.FailureSummary);
        Assert.NotNull(export.Graph);
        Assert.Single(export.Graph.Nodes);
        Assert.Empty(export.Graph.Edges);
    }
}