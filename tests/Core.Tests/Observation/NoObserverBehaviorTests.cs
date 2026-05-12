using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Observation;

public class NoObserverBehaviorTests
{
    [Fact]
    public async Task NoObserverPreservesSuccessfulOutcome()
    {
        var outcome = await ObservationTestDomain.Create().ApplyAsync(ObservationState.A, new Go());

        Assert.Equal(TransitionOutcomeCategory.Success, outcome.Category);
        Assert.True(outcome.Committed);
        Assert.Equal(ObservationState.B, outcome.ResultingState);
    }

    [Theory]
    [MemberData(nameof(Events))]
    internal async Task NoObserverPreservesNonSuccessCategories(ObservationEvent @event,
        TransitionOutcomeCategory expected)
    {
        var outcome = await ObservationTestDomain.Create().ApplyAsync(ObservationState.A, @event);

        Assert.Equal(expected, outcome.Category);
    }

    public static TheoryData<ObservationEvent, TransitionOutcomeCategory> Events()
    {
        return new TheoryData<ObservationEvent, TransitionOutcomeCategory>
        {
            { new Deny(), TransitionOutcomeCategory.Denied },
            { new FailBefore(), TransitionOutcomeCategory.BehaviorFailure },
            { new Missing(), TransitionOutcomeCategory.NotPermitted }
        };
    }
}