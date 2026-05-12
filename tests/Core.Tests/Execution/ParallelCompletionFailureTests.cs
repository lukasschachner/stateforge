using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Tests.Parallel;

namespace StateMachineLibrary.Core.Tests.Execution;

public sealed class ParallelCompletionFailureTests
{
    [Fact]
    public async Task Regional_action_failure_does_not_commit_partial_parallel_shape()
    {
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(b =>
        {
            b.ParallelComposite(ParallelState.Operational)
                .Region("Fulfillment", ParallelState.WaitingForPick, ParallelState.Packing)
                .Region("Billing", ParallelState.WaitingForPayment, ParallelState.CapturingPayment);
            b.State(ParallelState.WaitingForPick).On(ParallelEvent.Cancel).GoTo(ParallelState.Packing);
            b.State(ParallelState.WaitingForPayment).On(ParallelEvent.Cancel)
                .Execute(_ => throw new InvalidOperationException("boom")).GoTo(ParallelState.CapturingPayment);
        });
        var runtime = definition.CreateRuntime(ParallelState.Operational);
        var outcome = await runtime.ApplyAsync(ParallelEvent.Cancel);
        Assert.False(outcome.Committed);
        runtime.ActiveStateShape.AssertRegion("Fulfillment", ParallelState.WaitingForPick);
        runtime.ActiveStateShape.AssertRegion("Billing", ParallelState.WaitingForPayment);
    }
}