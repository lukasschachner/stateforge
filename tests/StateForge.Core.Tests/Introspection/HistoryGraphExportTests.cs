using StateForge.Core.Tests.History;
using StateForge.Core.Introspection;

namespace StateForge.Core.Tests.Introspection;

public class HistoryGraphExportTests
{
    [Fact]
    public void GraphExportIncludesHistoryMarkersAndNodeMetadata()
    {
        var graph = Assert.IsType<DefinitionGraph<HistoryState, HistoryEvent>>(HistoryTestDomain
            .CreateOperationalMachine().ExportGraph().Graph);

        Assert.Equal(1, graph.Hierarchy.HistoryMarkerCount);
        var marker = Assert.Single(graph.HistoryMarkers);
        Assert.Equal(HistoryState.Operational, marker.CompositeState);
        Assert.Equal("Shallow", marker.HistoryMode);
        Assert.Equal(HistoryState.Idle, marker.FallbackTargetState);

        var node = graph.Nodes.Single(n => n.State == HistoryState.Operational);
        Assert.True(node.HasHistory);
        Assert.Equal("Shallow", node.HistoryMode);
    }
}