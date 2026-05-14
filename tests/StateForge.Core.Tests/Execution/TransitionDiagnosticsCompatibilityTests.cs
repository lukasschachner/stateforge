using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;
using StateForge.Core.Tests.Parallel;

namespace StateForge.Core.Tests.Execution;

public sealed class TransitionDiagnosticsCompatibilityTests
{
    [Fact]
    public async Task Readable_parallel_conflict_summary_remains_available_with_structured_diagnostics()
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

        var outcome = await definition.CreateRuntime(ParallelState.Operational).ApplyAsync(ParallelEvent.Cancel);

        Assert.Contains("Parent-level transition conflicts", outcome.Diagnostics.Summary);
        Assert.Equal(outcome.Diagnostics.ConflictDiagnostics, outcome.ConflictDiagnostics);
        Assert.Equal(TransitionConflictKind.ParentRegionalConflict, outcome.ConflictDiagnostics.Single().Kind);
    }
}
