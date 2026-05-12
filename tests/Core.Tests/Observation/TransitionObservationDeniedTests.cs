using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Observation;

public class TransitionObservationDeniedTests
{
    [Fact]
    public async Task ConditionDeniedEmitsDeniedThenOutcome()
    {
        var kinds = await ObservationTestDomain.ObserveKindsAsync(new Deny());

        Assert.Equal(
        [
            TransitionObservationKind.Started, TransitionObservationKind.ConditionDenied,
            TransitionObservationKind.Outcome
        ], kinds);
    }

    [Fact]
    public async Task NotPermittedEmitsNotPermittedThenOutcome()
    {
        var kinds = await ObservationTestDomain.ObserveKindsAsync(new Missing());

        Assert.Equal(
        [
            TransitionObservationKind.Started, TransitionObservationKind.NotPermitted, TransitionObservationKind.Outcome
        ], kinds);
    }
}