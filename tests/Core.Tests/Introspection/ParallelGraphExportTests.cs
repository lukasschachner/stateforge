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
}