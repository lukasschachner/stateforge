using StateForge.Core.Execution;
using StateForge.OpenTelemetry;

namespace StateForge.OpenTelemetry.Tests.Instrumentation;

public class TransitionActivityFailureDiagnosticsTests
{
    [Theory]
    [MemberData(nameof(FailureEvents))]
    public async Task FailureDiagnosticsCanRecordExceptionTags(TelemetryEvent @event,
        TransitionOutcomeCategory expected)
    {
        using var capture = new ActivityCapture();
        using var observer =
            new StateMachineTelemetryObserver<TelemetryState, TelemetryEvent>(new StateMachineTelemetryOptions
            { RecordExceptionDetails = true });

        var outcome = await TelemetryTestDomain.Create().ApplyAsync(TelemetryState.Draft, @event, observer: observer);

        Assert.Equal(expected, outcome.Category);
        var activity = Assert.Single(capture.Activities);
        Assert.NotNull(activity.GetTagItem(StateMachineTelemetryNames.ErrorTypeAttribute));
        Assert.NotNull(activity.GetTagItem(StateMachineTelemetryNames.ErrorMessageAttribute));
    }

    public static TheoryData<TelemetryEvent, TransitionOutcomeCategory> FailureEvents()
    {
        return new TheoryData<TelemetryEvent, TransitionOutcomeCategory>
        {
            { new FailSubmit(), TransitionOutcomeCategory.BehaviorFailure },
            { new CancelSubmit(), TransitionOutcomeCategory.Cancelled }
        };
    }
}