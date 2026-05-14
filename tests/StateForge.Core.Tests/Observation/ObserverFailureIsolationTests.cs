using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Observation;

public class ObserverFailureIsolationTests
{
    [Fact]
    public async Task ObserverExceptionsDoNotAlterTransitionOutcome()
    {
        var observer =
            new RecordingTransitionObserver<ObservationState, ObservationEvent>(_ =>
                throw new InvalidOperationException("observer"));

        var outcome = await ObservationTestDomain.Create().ApplyAsync(ObservationState.A, new Go(), observer: observer);

        Assert.Equal(TransitionOutcomeCategory.Success, outcome.Category);
        Assert.Equal(4, observer.Observations.Count);
        Assert.DoesNotContain("observer", outcome.Diagnostics.Summary, StringComparison.OrdinalIgnoreCase);
    }
}