using StateForge.Core.Definitions;
using StateForge.Core.Execution;
using StateForge.Core.Tests.History;

namespace StateForge.Core.Tests.Observation;

public sealed class ParallelHistoryObservationOrderingTests
{
    [Fact]
    public async Task Restore_setup_preserves_regional_commit_observation_order()
    {
        var observer = new Observer();
        var runtime = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Shallow)
            .CreateRuntime(ParallelHistoryState.Operational, observer: observer);

        await runtime.ApplyAsync(ParallelHistoryEvent.PickStarted);
        await runtime.ApplyAsync(ParallelHistoryEvent.PaymentStarted);

        Assert.Equal([ParallelHistoryState.WaitingForPick, ParallelHistoryState.WaitingForPayment],
            observer.CommittedSources);
    }

    private sealed class Observer : ITransitionObserver<ParallelHistoryState, ParallelHistoryEvent>
    {
        public List<ParallelHistoryState> CommittedSources { get; } = [];

        public ValueTask ObserveAsync(TransitionObservation<ParallelHistoryState, ParallelHistoryEvent> observation,
            CancellationToken cancellationToken)
        {
            if (observation.Kind == TransitionObservationKind.Committed) CommittedSources.Add(observation.SourceState);

            return ValueTask.CompletedTask;
        }
    }
}