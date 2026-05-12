using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.OpenTelemetry;

namespace OpenTelemetry.Tests.Instrumentation;

public class TransitionActivitySuccessTests
{
    [Fact]
    public async Task SuccessfulTransitionCreatesActivityWithTags()
    {
        using var capture = new ActivityCapture();
        using var observer = new StateMachineTelemetryObserver<TelemetryState, TelemetryEvent>();

        var outcome = await TelemetryTestDomain.Create()
            .ApplyAsync(TelemetryState.Draft, new Submit(), observer: observer);

        Assert.Equal(TransitionOutcomeCategory.Success, outcome.Category);
        var activity = Assert.Single(capture.Activities);
        Assert.Equal(StateMachineTelemetryNames.TransitionActivityName, activity.OperationName);
        Assert.Equal("Draft", activity.GetTagItem(StateMachineTelemetryNames.SourceStateAttribute));
        Assert.Equal("Submitted", activity.GetTagItem(StateMachineTelemetryNames.ResultingStateAttribute));
        Assert.Equal("Success", activity.GetTagItem(StateMachineTelemetryNames.OutcomeAttribute));
        Assert.Equal(true, activity.GetTagItem(StateMachineTelemetryNames.CommittedAttribute));
    }
}