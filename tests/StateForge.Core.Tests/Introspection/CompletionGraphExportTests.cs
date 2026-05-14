using StateForge.Core.Introspection;
using StateForge.Core.Tests.Completion;

namespace StateForge.Core.Tests.Introspection;

public sealed class CompletionGraphExportTests
{
    [Fact]
    public void Graph_export_classifies_completion_edges_without_label_parsing()
    {
        var definition = OrdinaryCompletionTestFixtures.CreateReviewingDefinition();

        var graph = definition.ExportGraph().Graph!;
        var edge = Assert.Single(graph.Edges, e => e.TriggerKind == GraphTriggerKind.Completion);

        Assert.Equal(CompletionState.Reviewing, edge.SourceState);
        Assert.Equal(CompletionState.Approved, edge.TargetState);
        Assert.True(edge.SourceIsComposite);
        Assert.False(edge.SourceIsParallel);
    }
}
