using Core.Tests.Actions;
using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Execution;

public class ActionExecutionOrderingTests
{
    [Fact]
    public async Task ExternalTransitionRunsExitTransitionEntryBeforeCommit()
    {
        var log = new List<string>();
        var definition = ActionTestDomain.CreateWithOrderedActions(log);

        var outcome = await definition.ApplyAsync(ActionState.Created, new Actions.Pay());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(ActionState.Paid, outcome.ResultingState);
        Assert.Equal(["exit Created", "transition Pay", "entry Paid"], log);
    }

    [Fact]
    public async Task SelfTransitionRunsExitTransitionEntry()
    {
        var log = new List<string>();
        var definition = StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created)
                .OnExit(_ => log.Add("exit"))
                .OnEntry(_ => log.Add("entry"))
                .On<Stay>()
                .Execute(_ => log.Add("transition"))
                .Self();
        });

        await definition.ApplyAsync(ActionState.Created, new Stay());

        Assert.Equal(["exit", "transition", "entry"], log);
    }

    [Fact]
    public async Task InternalTransitionRunsTransitionActionsOnly()
    {
        var log = new List<string>();
        var definition = StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created)
                .OnExit(_ => log.Add("exit"))
                .OnEntry(_ => log.Add("entry"))
                .On<Stay>()
                .Execute(_ => log.Add("transition"))
                .Internal();
        });

        await definition.ApplyAsync(ActionState.Created, new Stay());

        Assert.Equal(["transition"], log);
    }
}