using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Tests.Completion;

namespace StateMachineLibrary.Core.Tests.Execution;

public sealed class OrdinaryCompletionOrderingTests
{
    [Fact]
    public async Task Terminal_entry_action_runs_before_completion_transition_action()
    {
        var log = new List<string>();
        var definition = StateMachineDefinition<CompletionState, CompletionEvent>.Create(builder =>
        {
            builder.State(CompletionState.Reviewing)
                .InitialChild(CompletionState.AuthorReview)
                .OnCompletion()
                .ExecuteAction(ctx => log.Add($"completion:{ctx.TriggerKind}"))
                .GoTo(CompletionState.Approved);
            builder.State(CompletionState.AuthorReview).ChildOf(CompletionState.Reviewing)
                .On(CompletionEvent.Approve).GoTo(CompletionState.ReviewDone);
            builder.State(CompletionState.ReviewDone).ChildOf(CompletionState.Reviewing).Terminal()
                .OnEntry(_ => log.Add("terminal-entry"));
            builder.State(CompletionState.Approved);
        });

        await definition.CreateRuntime(CompletionState.Reviewing).ApplyAsync(CompletionEvent.Approve);

        Assert.Equal(["terminal-entry", "completion:Completion"], log);
    }
}
