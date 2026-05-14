using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.History;

internal static class ParallelHistoryGraphTestData
{
    public static StateMachineDefinition<ParallelHistoryState, ParallelHistoryEvent> ShallowDefinition()
    {
        return ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Shallow);
    }

    public static StateMachineDefinition<ParallelHistoryState, ParallelHistoryEvent> DeepDefinition()
    {
        return ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Deep);
    }
}