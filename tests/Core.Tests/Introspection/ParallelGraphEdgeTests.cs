using StateMachineLibrary.Core.Tests.Parallel;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class ParallelGraphEdgeTests
{
    [Fact]
    public void Graph_edges_are_classified_as_regional()
    {
        var graph = ParallelGraphTestData.CreateTwoRegionDefinition().ExportGraph().Graph!;
        Assert.All(graph.Edges, edge => Assert.Equal("Regional", edge.RegionClassification));
    }
}