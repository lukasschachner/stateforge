using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Diagnostics;
using StateMachineLibrary.Core.Tests.Parallel;

namespace StateMachineLibrary.Core.Tests.Execution;

public sealed class ParallelCompletionFailureTests
{
    [Fact]
    public async Task Parallel_completion_conflict_returns_structured_diagnostic()
    {
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(b =>
        {
            b.ParallelComposite(ParallelState.Operational)
                .Region("Fulfillment", ParallelState.WaitingForPick, [ParallelState.FulfillmentDone], [ParallelState.FulfillmentDone])
                .Region("Billing", ParallelState.WaitingForPayment, [ParallelState.BillingDone], [ParallelState.BillingDone])
                .OnCompletion().When(_ => true, "ready").GoTo(ParallelState.Cancelled)
                .OnCompletion().When(_ => true, "also ready").GoTo(ParallelState.Idle);
            b.State(ParallelState.WaitingForPick).On(ParallelEvent.CompleteFulfillment).GoTo(ParallelState.FulfillmentDone);
            b.State(ParallelState.WaitingForPayment).On(ParallelEvent.CompleteBilling).GoTo(ParallelState.BillingDone);
            b.State(ParallelState.Cancelled);
            b.State(ParallelState.Idle);
        });
        var runtime = definition.CreateRuntime(ParallelState.Operational);
        await runtime.ApplyAsync(ParallelEvent.CompleteFulfillment);

        var outcome = await runtime.ApplyAsync(ParallelEvent.CompleteBilling);

        Assert.False(outcome.Committed);
        Assert.Equal(TransitionConflictKind.CompletionConflict,
            Assert.Single(outcome.Diagnostics.ConflictDiagnostics).Kind);
    }

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