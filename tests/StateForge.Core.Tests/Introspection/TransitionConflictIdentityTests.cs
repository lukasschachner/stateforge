using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;
using StateForge.Core.Tests.Parallel;

namespace StateForge.Core.Tests.Introspection;

public sealed class TransitionConflictIdentityTests
{
    [Fact]
    public async Task Runtime_conflict_transition_ids_align_with_graph_edge_ids()
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
        var graph = definition.ExportGraph();
        var runtime = definition.CreateRuntime(ParallelState.Operational);

        var outcome = await runtime.ApplyAsync(ParallelEvent.Cancel);

        Assert.True(graph.Succeeded);
        var edgeIds = graph.Graph!.Edges.Select(edge => edge.Id).ToHashSet(StringComparer.Ordinal);
        var conflict = Assert.Single(outcome.Diagnostics.ConflictDiagnostics);
        Assert.Equal(TransitionConflictKind.ParentRegionalConflict, conflict.Kind);
        Assert.All(conflict.Participants, participant => Assert.Contains(participant.TransitionId!, edgeIds));
    }
}
