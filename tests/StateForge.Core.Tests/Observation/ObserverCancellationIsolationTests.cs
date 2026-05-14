using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Observation;

public class ObserverCancellationIsolationTests
{
    [Fact]
    public async Task ObserverCancellationExceptionsDoNotCancelTransition()
    {
        var observer =
            new RecordingTransitionObserver<ObservationState, ObservationEvent>(_ =>
                throw new OperationCanceledException());

        var outcome = await ObservationTestDomain.Create().ApplyAsync(ObservationState.A, new Go(), observer: observer);

        Assert.Equal(TransitionOutcomeCategory.Success, outcome.Category);
        Assert.True(outcome.Committed);
    }
}