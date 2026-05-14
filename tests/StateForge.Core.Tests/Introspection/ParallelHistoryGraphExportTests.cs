using StateForge.Core.Definitions;
using StateForge.Core.Tests.History;

namespace StateForge.Core.Tests.Introspection;

public sealed class ParallelHistoryGraphExportTests
{
    [Fact]
    public void Graph_export_includes_parallel_history_mode_and_fallback_metadata()
    {
        var export = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Shallow).ExportGraph();
        Assert.True(export.Succeeded);

        Assert.NotNull(export.Graph);
        var fulfillment = Assert.Single(export.Graph!.Regions, r => r.RegionName == "Fulfillment");
        Assert.True(fulfillment.ParallelHistorySupported);
        Assert.Equal("Shallow", fulfillment.ParallelHistoryMode);
        Assert.Equal(ParallelHistoryState.WaitingForPick, fulfillment.ParallelHistoryFallbackState);
    }
}