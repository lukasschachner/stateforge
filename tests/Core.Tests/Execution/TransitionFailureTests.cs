using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Execution;

public class TransitionFailureTests
{
    [Fact]
    public async Task PreCommitBehaviorFailurePreservesState()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>().Execute(_ => throw new InvalidOperationException("boom"))
                .GoTo(OrderState.Paid);
            builder.State(OrderState.Paid);
        });

        var outcome = await definition.ApplyAsync(OrderState.Created, new Pay());

        Assert.Equal(TransitionOutcomeCategory.BehaviorFailure, outcome.Category);
        Assert.Equal(OrderState.Created, outcome.ResultingState);
        Assert.Equal(TransitionLifecyclePhase.Transition, outcome.Diagnostics.Phase);
        Assert.NotNull(outcome.Transition);
        Assert.False(outcome.Committed);
    }

    [Fact]
    public async Task EntryFailureBeforeCommitPreservesState()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>().OnEntry(_ => throw new InvalidOperationException("entry"))
                .GoTo(OrderState.Paid);
            builder.State(OrderState.Paid);
        });

        var outcome = await definition.ApplyAsync(OrderState.Created, new Pay());

        Assert.Equal(TransitionOutcomeCategory.BehaviorFailure, outcome.Category);
        Assert.Equal(OrderState.Created, outcome.ResultingState);
        Assert.Equal(TransitionLifecyclePhase.Entry, outcome.Diagnostics.Phase);
        Assert.False(outcome.Committed);
    }

    [Fact]
    public async Task CancellationBeforeCommitPreservesState()
    {
        var cts = new CancellationTokenSource();
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>().Execute(_ => cts.Cancel()).GoTo(OrderState.Paid);
            builder.State(OrderState.Paid);
        });

        var outcome = await definition.ApplyAsync(OrderState.Created, new Pay(), cts.Token);

        Assert.Equal(TransitionOutcomeCategory.Cancelled, outcome.Category);
        Assert.Equal(OrderState.Created, outcome.ResultingState);
        Assert.Equal(TransitionLifecyclePhase.Commit, outcome.Diagnostics.Phase);
        Assert.False(outcome.Committed);
    }

    [Fact]
    public async Task RuntimePreservesStateWhenEntryFailureOccursBeforeCommit()
    {
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>().OnEntry(_ => throw new InvalidOperationException("entry"))
                .GoTo(OrderState.Paid);
            builder.State(OrderState.Paid);
        });
        var runtime = definition.CreateRuntime(OrderState.Created);

        var outcome = await runtime.ApplyAsync(new Pay());

        Assert.Equal(TransitionOutcomeCategory.BehaviorFailure, outcome.Category);
        Assert.False(outcome.Committed);
        Assert.Equal(OrderState.Created, runtime.CurrentState);
    }
}