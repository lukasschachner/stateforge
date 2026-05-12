using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Observation;

public class TransitionObservationOrderingTests
{
    [Fact]
    public async Task SuccessfulTransitionEmitsStartedCommittedCompletedOutcome()
    {
        var observer = new RecordingTransitionObserver<ObservationState, ObservationEvent>();

        var outcome = await ObservationTestDomain.Create().ApplyAsync(ObservationState.A, new Go(), observer: observer);

        Assert.True(outcome.IsSuccess);
        Assert.Equal(
            [
                TransitionObservationKind.Started, TransitionObservationKind.Committed,
                TransitionObservationKind.Completed, TransitionObservationKind.Outcome
            ],
            observer.Observations.Select(o => o.Kind));
        Assert.Single(observer.Observations.Select(o => o.AttemptId).Distinct());
        Assert.All(observer.Observations, o => Assert.Equal(ObservationState.A, o.SourceState));
        Assert.Equal(ObservationState.B, observer.Observations.Last().ResultingState);
    }
}