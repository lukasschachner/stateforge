using StateMachineLibrary.Core.Introspection;
using StateMachineLibrary.Core.Tests.Completion;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class ParallelCompletionGraphExportTests
{
    [Fact]
    public void Parallel_completion_graph_edge_identifies_parallel_source()
    {
        var definition = ParallelCompletionTestFixtures.CreateOperationalDefinition();

        var edge = Assert.Single(definition.ExportGraph().Graph!.Edges, e => e.TriggerKind == GraphTriggerKind.Completion);

        Assert.Equal(CompletionState.Operational, edge.SourceState);
        Assert.True(edge.SourceIsParallel);
    }
}
