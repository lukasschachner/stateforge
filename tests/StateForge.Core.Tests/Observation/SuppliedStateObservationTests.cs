using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Observation;

public class SuppliedStateObservationTests
{
    [Fact]
    public async Task SuppliedStateApplyAcceptsObserverWithoutOwningState()
    {
        var observer = new RecordingTransitionObserver<ObservationState, ObservationEvent>();

        var outcome = await ObservationTestDomain.Create().ApplyAsync(ObservationState.A, new Go(), observer: observer);

        Assert.True(outcome.IsSuccess);
        Assert.Equal(ObservationState.B, outcome.ResultingState);
        Assert.Contains(observer.Observations, o => o.Kind == TransitionObservationKind.Outcome);
    }
}