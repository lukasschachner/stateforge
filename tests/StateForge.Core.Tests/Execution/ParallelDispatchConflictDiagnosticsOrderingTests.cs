using StateForge.Core.Definitions;
using StateForge.Core.Execution;
using StateForge.Core.Tests.Parallel;

namespace StateForge.Core.Tests.Execution;

public sealed class ParallelDispatchConflictDiagnosticsOrderingTests
{
    [Fact]
    public async Task Parent_regional_conflict_participant_order_is_stable_across_runs()
    {
        var definition = StateMachineDefinition<ParallelState, ParallelEvent>.Create(builder =>
        {
            builder.ParallelComposite(ParallelState.Operational)
                .Region("Fulfillment", ParallelState.WaitingForPick, ParallelState.WaitingForPick,
                    ParallelState.Packing)
                .Region("Billing", ParallelState.WaitingForPayment, ParallelState.WaitingForPayment,
                    ParallelState.CapturingPayment);
            builder.State(ParallelState.Operational).On(ParallelEvent.Cancel).GoTo(ParallelState.Cancelled);
            builder.State(ParallelState.WaitingForPick).On(ParallelEvent.Cancel).GoTo(ParallelState.Packing);
            builder.State(ParallelState.WaitingForPayment).On(ParallelEvent.Cancel).GoTo(ParallelState.CapturingPayment);
            builder.State(ParallelState.Cancelled);
        });

        var preview = await definition.CreateRuntime(ParallelState.Operational).PreviewAsync(ParallelEvent.Cancel);
        Assert.Equal(TransitionPreviewStatus.Denied, preview.Status);
        Assert.Single(preview.ConflictDiagnostics);

        string[]? expected = null;
        for (var i = 0; i < 10; i++)
        {
            var outcome = await definition.CreateRuntime(ParallelState.Operational).ApplyAsync(ParallelEvent.Cancel);
            var actual = outcome.Diagnostics.ConflictDiagnostics.Single().Participants
                .Select(participant => $"{participant.Role}:{participant.TransitionId}:{participant.RegionName}")
                .ToArray();
            expected ??= actual;
            Assert.Equal(expected, actual);
        }
    }
}
