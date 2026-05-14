using StateForge.Core.Tests.History;
using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Execution;

public static class HistoryExecutionFixtures
{
    public static StateMachineRuntime<HistoryState, HistoryEvent> CreateOperationalRuntime(List<string>? log = null)
    {
        return HistoryTestDomain.CreateOperationalMachine(log)
            .CreateRuntime(HistoryState.Offline, ConcurrencyMode.Serialized);
    }
}