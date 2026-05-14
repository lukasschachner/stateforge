using StateForge.Core.Tests.Actions;
using StateForge.Core.Definitions;
using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Execution;

public class ActionFailureTests
{
    [Theory]
    [InlineData("exit")]
    [InlineData("transition")]
    [InlineData("entry")]
    public async Task ActionFailuresPreserveSourceStateAndSkipLaterActions(string failingPhase)
    {
        var log = new List<string>();
        var definition = StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created)
                .OnExit(_ => FailIf(failingPhase, "exit", log), "exit action")
                .On<Actions.Pay>()
                .Execute(_ => FailIf(failingPhase, "transition", log), "transition action")
                .GoTo(ActionState.Paid);
            builder.State(ActionState.Paid)
                .OnEntry(_ => FailIf(failingPhase, "entry", log), "entry action")
                .OnEntry(_ => log.Add("entry after"), "entry after");
        });

        var outcome = await definition.ApplyAsync(ActionState.Created, new Actions.Pay());

        Assert.Equal(TransitionOutcomeCategory.BehaviorFailure, outcome.Category);
        Assert.Equal(ActionState.Created, outcome.ResultingState);
        Assert.False(outcome.Committed);
        Assert.Contains(failingPhase, outcome.Diagnostics.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("entry after", log);
    }

    private static void FailIf(string failingPhase, string phase, IList<string> log)
    {
        log.Add(phase);
        if (failingPhase == phase) throw new InvalidOperationException(phase);
    }
}