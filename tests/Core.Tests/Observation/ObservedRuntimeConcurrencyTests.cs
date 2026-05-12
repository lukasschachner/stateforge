using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Observation;

public class ObservedRuntimeConcurrencyTests
{
    [Theory]
    [InlineData(ConcurrencyMode.Fast)]
    [InlineData(ConcurrencyMode.Serialized)]
    public async Task ObservedRuntimeStillAppliesCommittedState(ConcurrencyMode mode)
    {
        var observer = new RecordingTransitionObserver<ObservationState, ObservationEvent>();
        var runtime = ObservationTestDomain.Create().CreateRuntime(ObservationState.A, mode, observer);

        var outcome = await runtime.ApplyAsync(new Go());

        Assert.True(outcome.Committed);
        Assert.Equal(ObservationState.B, runtime.CurrentState);
        Assert.Contains(observer.Observations, o => o.Kind == TransitionObservationKind.Outcome);
    }
}