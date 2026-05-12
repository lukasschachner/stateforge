using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Execution;

public class SpecialTransitionTests
{
    [Fact]
    public async Task SelfTransitionRunsExitTransitionAndEntry()
    {
        var phases = new List<string>();
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>()
                .OnExit(_ => phases.Add("exit"))
                .Execute(_ => phases.Add("transition"))
                .OnEntry(_ => phases.Add("entry"))
                .Self();
        });

        var outcome = await definition.ApplyAsync(OrderState.Created, new Pay());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(OrderState.Created, outcome.ResultingState);
        Assert.Equal(["exit", "transition", "entry"], phases);
    }

    [Fact]
    public async Task InternalTransitionRunsOnlyTransitionBehavior()
    {
        var phases = new List<string>();
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>()
                .OnExit(_ => phases.Add("exit"))
                .Execute(_ => phases.Add("transition"))
                .OnEntry(_ => phases.Add("entry"))
                .Internal();
        });

        var outcome = await definition.ApplyAsync(OrderState.Created, new Pay());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(OrderState.Created, outcome.ResultingState);
        Assert.Equal(["transition"], phases);
    }
}