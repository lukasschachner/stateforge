using StateForge.Core.Tests.Actions;
using StateForge.Core.Definitions;
using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Execution;

public class ActionCancellationTests
{
    [Theory]
    [InlineData("exit")]
    [InlineData("transition")]
    [InlineData("entry")]
    public async Task ActionCancellationPreservesSourceState(string cancellingPhase)
    {
        using var cts = new CancellationTokenSource();
        var definition = StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created)
                .OnExit(ctx => CancelIf(cancellingPhase, "exit", ctx.CancellationToken))
                .On<Actions.Pay>()
                .Execute(ctx => CancelIf(cancellingPhase, "transition", ctx.CancellationToken))
                .GoTo(ActionState.Paid);
            builder.State(ActionState.Paid)
                .OnEntry(ctx => CancelIf(cancellingPhase, "entry", ctx.CancellationToken));
        });

        var outcome = await definition.ApplyAsync(ActionState.Created, new Actions.Pay(), cts.Token);

        Assert.Equal(TransitionOutcomeCategory.Cancelled, outcome.Category);
        Assert.Equal(ActionState.Created, outcome.ResultingState);
        Assert.False(outcome.Committed);
    }

    private static void CancelIf(string cancellingPhase, string phase, CancellationToken cancellationToken)
    {
        if (cancellingPhase == phase) throw new OperationCanceledException(cancellationToken);
    }
}