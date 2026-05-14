using StateForge.Core.Tests.History;
using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Observation;

public class HistoryObservationOrderingTests
{
    [Fact]
    public async Task HistoryTransitionsStillNotifyCommitBeforeCompletedOutcome()
    {
        var observer = new RecordingTransitionObserver<HistoryState, HistoryEvent>();
        var runtime = HistoryTestDomain.CreateOperationalMachine()
            .CreateRuntime(HistoryState.Offline, ConcurrencyMode.Serialized, observer);

        await runtime.ApplyAsync(new Resume());

        var kinds = observer.Observations.Select(o => o.Kind).ToArray();
        Assert.Contains(TransitionObservationKind.Committed, kinds);
        Assert.Contains(TransitionObservationKind.Completed, kinds);
        Assert.True(Array.IndexOf(kinds, TransitionObservationKind.Committed) <
                    Array.IndexOf(kinds, TransitionObservationKind.Completed));
    }
}