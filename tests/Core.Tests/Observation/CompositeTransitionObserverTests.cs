using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Observation;

public class CompositeTransitionObserverTests
{
    [Fact]
    public async Task CompositeForwardsNotificationsToAllObserversInOrder()
    {
        var first = new RecordingTransitionObserver<ObservationState, ObservationEvent>();
        var second = new RecordingTransitionObserver<ObservationState, ObservationEvent>();
        var composite = new CompositeTransitionObserver<ObservationState, ObservationEvent>(first, second);

        await ObservationTestDomain.Create().ApplyAsync(ObservationState.A, new Go(), observer: composite);

        Assert.Equal(first.Observations.Select(o => o.Kind), second.Observations.Select(o => o.Kind));
        Assert.Equal(
        [
            TransitionObservationKind.Started, TransitionObservationKind.Committed, TransitionObservationKind.Completed,
            TransitionObservationKind.Outcome
        ], first.Observations.Select(o => o.Kind));
    }

    [Fact]
    public async Task CompositeSuppressesFailingChildObserversAndContinuesFanOut()
    {
        var failing =
            new RecordingTransitionObserver<ObservationState, ObservationEvent>(_ =>
                throw new InvalidOperationException("child"));
        var succeeding = new RecordingTransitionObserver<ObservationState, ObservationEvent>();
        var composite = new CompositeTransitionObserver<ObservationState, ObservationEvent>(failing, succeeding);

        var outcome = await ObservationTestDomain.Create()
            .ApplyAsync(ObservationState.A, new Go(), observer: composite);

        Assert.Equal(TransitionOutcomeCategory.Success, outcome.Category);
        Assert.Equal(4, failing.Observations.Count);
        Assert.Equal(4, succeeding.Observations.Count);
    }
}