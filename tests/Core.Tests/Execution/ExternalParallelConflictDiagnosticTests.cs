using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Diagnostics;
using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Core.Tests.Parallel;

namespace StateMachineLibrary.Core.Tests.Execution;

public sealed class ExternalParallelConflictDiagnosticTests
{
    [Fact]
    public async Task External_parallel_conflict_returns_diagnostics_without_state_write()
    {
        var state = ParallelState.Operational;
        var writes = 0;
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(builder =>
        {
            builder.ParallelComposite(ParallelState.Operational)
                .Region("Fulfillment", ParallelState.WaitingForPick, ParallelState.WaitingForPick,
                    ParallelState.Packing)
                .Region("Billing", ParallelState.WaitingForPayment, ParallelState.WaitingForPayment);
            builder.State(ParallelState.Operational).On(ParallelEvent.Cancel).GoTo(ParallelState.Cancelled);
            builder.State(ParallelState.WaitingForPick).On(ParallelEvent.Cancel).GoTo(ParallelState.Packing);
            builder.State(ParallelState.Cancelled);
        });
        var runtime = definition.CreateRuntime(StateAccessor.Create(() => state, next =>
        {
            writes++;
            state = next;
        }));

        var outcome = await runtime.ApplyAsync(ParallelEvent.Cancel);

        Assert.False(outcome.Committed);
        Assert.Equal(ParallelState.Operational, state);
        Assert.Equal(0, writes);
        Assert.Equal(TransitionConflictKind.ParentRegionalConflict,
            Assert.Single(outcome.Diagnostics.ConflictDiagnostics).Kind);
    }
}
