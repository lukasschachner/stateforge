using System.Diagnostics;
using StateForge.Core.Execution;
using StateForge.OpenTelemetry;

namespace StateForge.OpenTelemetry.Tests.Instrumentation;

public class TransitionActivityNonSuccessTests
{
    [Theory]
    [MemberData(nameof(NonSuccessEvents))]
    public async Task NonSuccessOutcomesAreTaggedDistinctly(TelemetryEvent @event, TransitionOutcomeCategory expected)
    {
        using var capture = new ActivityCapture();
        using var observer = new StateMachineTelemetryObserver<TelemetryState, TelemetryEvent>();

        var outcome = await TelemetryTestDomain.Create().ApplyAsync(TelemetryState.Draft, @event, observer: observer);

        Assert.Equal(expected, outcome.Category);
        var activity = Assert.Single(capture.Activities);
        Assert.Equal(expected.ToString(), activity.GetTagItem(StateMachineTelemetryNames.OutcomeAttribute));
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
    }

    [Fact]
    public async Task ValidationFailureIsTaggedDistinctly()
    {
        using var capture = new ActivityCapture();
        using var observer = new StateMachineTelemetryObserver<TelemetryState, TelemetryEvent>();

        var outcome = await TelemetryTestDomain.CreateInvalid()
            .ApplyAsync(TelemetryState.Draft, new Submit(), observer: observer);

        Assert.Equal(TransitionOutcomeCategory.ValidationFailure, outcome.Category);
        var activity = Assert.Single(capture.Activities);
        Assert.Equal("ValidationFailure", activity.GetTagItem(StateMachineTelemetryNames.OutcomeAttribute));
    }

    public static TheoryData<TelemetryEvent, TransitionOutcomeCategory> NonSuccessEvents()
    {
        return new TheoryData<TelemetryEvent, TransitionOutcomeCategory>
        {
            { new DenySubmit(), TransitionOutcomeCategory.Denied },
            { new MissingTelemetryEvent(), TransitionOutcomeCategory.NotPermitted }
        };
    }
}