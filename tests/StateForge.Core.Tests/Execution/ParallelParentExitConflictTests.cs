using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;
using StateForge.Core.Tests.Parallel;

namespace StateForge.Core.Tests.Execution;

public sealed class ParallelParentExitConflictTests
{
    [Fact]
    public async Task Parent_exit_and_regional_transition_conflict_before_commit()
    {
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
        var runtime = definition.CreateRuntime(ParallelState.Operational);

        var outcome = await runtime.ApplyAsync(ParallelEvent.Cancel);

        Assert.False(outcome.Committed);
        Assert.Contains("Parent-level", outcome.Diagnostics.Summary);
        var conflict = Assert.Single(outcome.Diagnostics.ConflictDiagnostics);
        Assert.Equal(TransitionConflictKind.ParentRegionalConflict, conflict.Kind);
        Assert.Equal(ParallelState.Operational, conflict.CompositeState);
        Assert.Collection(conflict.Participants,
            parent => Assert.Equal(TransitionConflictParticipantRole.ParentTransition, parent.Role),
            regional =>
            {
                Assert.Equal(TransitionConflictParticipantRole.RegionalTransition, regional.Role);
                Assert.Equal("Fulfillment", regional.RegionName);
            });
    }
}