using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Observation;

public class TransitionObservationValidationTests
{
    [Fact]
    public async Task ValidationFailureEmitsValidationFailureThenOutcome()
    {
        var observer = new RecordingTransitionObserver<ObservationState, ObservationEvent>();

        var outcome = await ObservationTestDomain.CreateInvalid()
            .ApplyAsync(ObservationState.A, new Go(), observer: observer);

        Assert.Equal(TransitionOutcomeCategory.ValidationFailure, outcome.Category);
        Assert.Equal(
        [
            TransitionObservationKind.Started, TransitionObservationKind.ValidationFailure,
            TransitionObservationKind.Outcome
        ], observer.Observations.Select(o => o.Kind));
        Assert.NotEmpty(observer.Observations.Last().Diagnostics.ValidationFindings);
    }
}