using Core.Tests.Actions;
using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Introspection;

public class GraphExportActionSummaryTests
{
    [Fact]
    public void GraphExportIncludesActionSummariesAndDoesNotExecuteActions()
    {
        var calls = 0;
        var definition = StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created)
                .OnExit(_ => calls++, "exit created")
                .On<Actions.Pay>()
                .Execute(_ => calls++, "transition pay")
                .GoTo(ActionState.Paid);
            builder.State(ActionState.Paid)
                .OnEntry(_ => calls++, "entry paid");
        });

        var export = definition.ExportGraph();

        Assert.True(export.Succeeded);
        Assert.Equal(0, calls);
        Assert.Equal("exit created",
            export.Graph!.Nodes.Single(n => n.State == ActionState.Created).ExitActions.Single().DisplayName);
        Assert.Equal("entry paid",
            export.Graph.Nodes.Single(n => n.State == ActionState.Paid).EntryActions.Single().DisplayName);
        Assert.Equal("transition pay", export.Graph.Edges.Single().TransitionActions.Single().DisplayName);
    }
}