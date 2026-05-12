using Core.Tests.History;
using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Execution;

public static class HistoryExecutionFixtures
{
    public static StateMachineRuntime<HistoryState, HistoryEvent> CreateOperationalRuntime(List<string>? log = null)
    {
        return HistoryTestDomain.CreateOperationalMachine(log)
            .CreateRuntime(HistoryState.Offline, ConcurrencyMode.Serialized);
    }
}