using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Introspection;

public class GraphExportNonExecutionTests
{
    [Fact]
    public void GraphExportDoesNotEvaluateConditionsOrExecuteBehaviors()
    {
        var conditionCalls = 0;
        var behaviorCalls = 0;
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created)
                .On<Pay>()
                .When(_ =>
                {
                    conditionCalls++;
                    return true;
                }, "must not run")
                .Execute(_ => behaviorCalls++, "must not run behavior")
                .GoTo(OrderState.Paid);
            builder.State(OrderState.Paid).Terminal();
        });

        var export = definition.ExportGraph();

        Assert.True(export.Succeeded, export.FailureSummary);
        Assert.Equal(0, conditionCalls);
        Assert.Equal(0, behaviorCalls);
        Assert.Equal("must not run", Assert.Single(export.Graph!.Edges).Conditions.Conditions.Single().DisplayName);
    }
}