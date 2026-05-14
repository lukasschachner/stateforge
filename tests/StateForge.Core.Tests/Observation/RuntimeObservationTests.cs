using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Observation;

public class RuntimeObservationTests
{
    [Fact]
    public async Task StateOwningRuntimePassesConfiguredObserver()
    {
        var observer = new RecordingTransitionObserver<ObservationState, ObservationEvent>();
        var runtime = ObservationTestDomain.Create().CreateRuntime(ObservationState.A, observer: observer);

        await runtime.ApplyAsync(new Go());

        Assert.Equal(ObservationState.B, runtime.CurrentState);
        Assert.Contains(observer.Observations, o => o.Kind == TransitionObservationKind.Outcome);
    }

    [Fact]
    public async Task ExternalStateRuntimePassesConfiguredObserver()
    {
        var state = ObservationState.A;
        var accessor = StateAccessor.Create(() => state, next => state = next);
        var observer = new RecordingTransitionObserver<ObservationState, ObservationEvent>();
        var runtime = ObservationTestDomain.Create().CreateRuntime(accessor, observer: observer);

        await runtime.ApplyAsync(new Go());

        Assert.Equal(ObservationState.B, state);
        Assert.Contains(observer.Observations, o => o.Kind == TransitionObservationKind.Outcome);
    }
}