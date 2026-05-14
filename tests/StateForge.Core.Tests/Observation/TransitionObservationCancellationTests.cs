using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Observation;

public class TransitionObservationCancellationTests
{
    [Fact]
    public async Task CancellationBeforeCommitEmitsCancelledThenOutcome()
    {
        using var cts = new CancellationTokenSource();
        var kinds = await ObservationTestDomain.ObserveKindsAsync(new CancelBefore(), cts.Token);

        Assert.Equal(
            [TransitionObservationKind.Started, TransitionObservationKind.Cancelled, TransitionObservationKind.Outcome],
            kinds);
    }

    [Fact]
    public async Task EntryCancellationBeforeCommitEmitsCancelledThenOutcome()
    {
        using var cts = new CancellationTokenSource();
        var kinds = await ObservationTestDomain.ObserveKindsAsync(new CancelAfter(), cts.Token);

        Assert.Equal(
            [TransitionObservationKind.Started, TransitionObservationKind.Cancelled, TransitionObservationKind.Outcome],
            kinds);
    }
}