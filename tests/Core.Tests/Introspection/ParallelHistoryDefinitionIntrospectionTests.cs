using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Tests.History;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class ParallelHistoryDefinitionIntrospectionTests
{
    [Fact]
    public void Definition_introspection_reports_parallel_history_modes_and_fallbacks()
    {
        var definition = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Deep);
        var introspection = definition.Introspect();

        Assert.Equal(HistoryMode.Deep, introspection.GetParallelHistoryMode(ParallelHistoryState.Operational));
        var metadata = Assert.Single(introspection.ParallelHistoryDefinitions);
        Assert.Equal(HistoryMode.Deep, metadata.HistoryMode);
        Assert.Equal(["Fulfillment", "Billing"], metadata.RegionFallbacks.Select(f => f.RegionName).ToArray());
        Assert.Equal(ParallelHistoryState.WaitingForPick, metadata.RegionFallbacks[0].FallbackState);
    }

    [Fact]
    public void Definition_introspection_reports_none_for_non_history_parallel_composite()
    {
        var definition = ParallelHistoryGraphTestData.ShallowDefinition();

        Assert.Equal(HistoryMode.Shallow,
            definition.Introspect().GetParallelHistoryMode(ParallelHistoryState.Operational));
        Assert.Equal(HistoryMode.None, definition.Introspect().GetParallelHistoryMode(ParallelHistoryState.Idle));
    }
}