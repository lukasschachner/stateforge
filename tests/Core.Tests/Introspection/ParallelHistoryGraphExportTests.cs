using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Tests.History;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class ParallelHistoryGraphExportTests
{
    [Fact]
    public void Graph_export_includes_parallel_history_mode_and_fallback_metadata()
    {
        var graph = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Shallow).ExportGraph().Graph;

        var fulfillment = Assert.Single(graph.Regions, r => r.RegionName == "Fulfillment");
        Assert.True(fulfillment.ParallelHistorySupported);
        Assert.Equal("Shallow", fulfillment.ParallelHistoryMode);
        Assert.Equal(ParallelHistoryState.WaitingForPick, fulfillment.ParallelHistoryFallbackState);
    }
}