using Core.Tests.Actions;
using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Observation;

public class ActionObservationTests
{
    [Fact]
    public async Task ActionFailureEmitsFailureOutcomeWithoutCompleted()
    {
        var observer = new RecordingTransitionObserver<ActionState, ActionEvent>();
        var definition = StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created)
                .OnExit(_ => throw new InvalidOperationException("exit"), "exit action")
                .On<Actions.Pay>()
                .GoTo(ActionState.Paid);
            builder.State(ActionState.Paid);
        });

        var outcome =
            await definition.ApplyAsync(ActionState.Created, new Actions.Pay(), observer: observer);

        Assert.Equal(TransitionOutcomeCategory.BehaviorFailure, outcome.Category);
        Assert.Equal(
        [
            TransitionObservationKind.Started, TransitionObservationKind.BehaviorFailed,
            TransitionObservationKind.Outcome
        ], observer.Observations.Select(o => o.Kind));
    }

    [Fact]
    public async Task SuccessfulActionTransitionEmitsCompletedAfterCommitted()
    {
        var observer = new RecordingTransitionObserver<ActionState, ActionEvent>();
        var log = new List<string>();
        var definition = ActionTestDomain.CreateWithOrderedActions(log);

        var outcome =
            await definition.ApplyAsync(ActionState.Created, new Actions.Pay(), observer: observer);

        Assert.True(outcome.IsSuccess);
        Assert.Equal(
        [
            TransitionObservationKind.Started, TransitionObservationKind.Committed, TransitionObservationKind.Completed,
            TransitionObservationKind.Outcome
        ], observer.Observations.Select(o => o.Kind));
    }
}