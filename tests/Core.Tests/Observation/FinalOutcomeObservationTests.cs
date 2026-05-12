using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Observation;

public class FinalOutcomeObservationTests
{
    [Theory]
    [MemberData(nameof(Events))]
    internal async Task EveryObservedAttemptEmitsExactlyOneFinalOutcome(ObservationEvent @event)
    {
        var observer = new RecordingTransitionObserver<ObservationState, ObservationEvent>();

        await ObservationTestDomain.Create().ApplyAsync(ObservationState.A, @event, observer: observer);

        Assert.Equal(1, observer.Observations.Count(o => o.Kind == TransitionObservationKind.Outcome));
        Assert.Equal(TransitionObservationKind.Outcome, observer.Observations.Last().Kind);
    }

    public static TheoryData<ObservationEvent> Events()
    {
        return new TheoryData<ObservationEvent>
        {
            new Go(), new Deny(), new FailBefore(), new FailAfter(), new Missing()
        };
    }
}