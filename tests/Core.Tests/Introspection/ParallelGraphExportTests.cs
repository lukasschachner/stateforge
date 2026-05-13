using StateMachineLibrary.Core.Tests.Parallel;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class ParallelGraphExportTests
{
    [Fact]
    public void Graph_export_contains_region_metadata()
    {
        var export = ParallelGraphTestData.CreateTwoRegionDefinition().ExportGraph();
        Assert.True(export.Succeeded);
        Assert.Equal(["Fulfillment", "Billing"], export.Graph!.Regions.Select(r => r.RegionName));
        Assert.Contains(export.Graph.Nodes, n => n.IsParallelComposite);
    }

    [Fact]
    public void Region_block_graph_export_matches_old_style_regions_and_edges()
    {
        var oldStyle = ParallelGraphTestData.CreateTwoRegionDefinitionOldStyle().ExportGraph().Graph!;
        var newStyle = ParallelGraphTestData.CreateTwoRegionDefinitionNewStyle().ExportGraph().Graph!;

        Assert.Equal(oldStyle.Regions.Select(r => (r.RegionName, r.CompositeState)),
            newStyle.Regions.Select(r => (r.RegionName, r.CompositeState)));
        Assert.Equal(oldStyle.Edges.Select(e => (e.SourceState, e.TargetState, Event: e.Event.DisplayName)),
            newStyle.Edges.Select(e => (e.SourceState, e.TargetState, Event: e.Event.DisplayName)));
    }
}