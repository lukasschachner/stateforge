using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Observation;

public class TransitionObservationFailureTests
{
    [Fact]
    public async Task BehaviorFailureBeforeCommitEmitsFailureThenOutcome()
    {
        var kinds = await ObservationTestDomain.ObserveKindsAsync(new FailBefore());

        Assert.Equal(
        [
            TransitionObservationKind.Started, TransitionObservationKind.BehaviorFailed,
            TransitionObservationKind.Outcome
        ], kinds);
    }

    [Fact]
    public async Task EntryBehaviorFailureBeforeCommitEmitsFailureThenOutcome()
    {
        var kinds = await ObservationTestDomain.ObserveKindsAsync(new FailAfter());

        Assert.Equal(
        [
            TransitionObservationKind.Started, TransitionObservationKind.BehaviorFailed,
            TransitionObservationKind.Outcome
        ], kinds);
    }
}