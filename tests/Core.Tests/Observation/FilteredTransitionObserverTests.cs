using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Observation;

public class FilteredTransitionObserverTests
{
    [Fact]
    public async Task FilteredObserverForwardsOnlyMatchingNotifications()
    {
        var recording = new RecordingTransitionObserver<ObservationState, ObservationEvent>();
        var filtered = new FilteredTransitionObserver<ObservationState, ObservationEvent>(
            recording,
            observation =>
                observation.Kind is TransitionObservationKind.Outcome or TransitionObservationKind.BehaviorFailed);

        await ObservationTestDomain.Create().ApplyAsync(ObservationState.A, new FailBefore(), observer: filtered);

        Assert.Equal([TransitionObservationKind.BehaviorFailed, TransitionObservationKind.Outcome],
            recording.Observations.Select(o => o.Kind));
    }

    [Fact]
    public async Task FilteredObserverSkipsInnerObserverWhenPredicateDoesNotMatch()
    {
        var recording = new RecordingTransitionObserver<ObservationState, ObservationEvent>();
        var filtered = new FilteredTransitionObserver<ObservationState, ObservationEvent>(recording, _ => false);

        await ObservationTestDomain.Create().ApplyAsync(ObservationState.A, new Go(), observer: filtered);

        Assert.Empty(recording.Observations);
    }
}