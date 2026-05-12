using Core.Tests.Observation;
using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Core.Tests.Completion;

namespace StateMachineLibrary.Core.Tests.Observation;

public sealed class CompletionObservationTests
{
    [Fact]
    public async Task Completion_transition_observations_are_classified_as_completion_triggered()
    {
        var observer = new RecordingTransitionObserver<CompletionState, CompletionEvent>();
        var definition = OrdinaryCompletionTestFixtures.CreateReviewingDefinition();
        var runtime = definition.CreateRuntime(CompletionState.Reviewing, observer: observer);

        await runtime.ApplyAsync(CompletionEvent.Approve);

        Assert.Contains(observer.Observations,
            observation => observation.Kind == TransitionObservationKind.Started &&
                           observation.TriggerKind == StateMachineLibrary.Core.Definitions.TransitionTriggerKind.Completion);
        Assert.Contains(observer.Observations,
            observation => observation.Kind == TransitionObservationKind.Completed && observation.IsCompletionTrigger);
    }
}
