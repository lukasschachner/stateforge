using Core.Tests.Hierarchy;
using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Observation;

public class HierarchyObservationOrderingTests
{
    [Fact]
    public async Task HierarchyTransitionObservationsIncludeResolvedActivePath()
    {
        var definition = HierarchyTestDomain.CreateReviewMachine();
        var observer = new RecordingTransitionObserver<HierarchyState, HierarchyEvent>();

        var outcome = await definition.ApplyAsync(HierarchyState.AuthorReview, new Submit(), observer: observer);

        Assert.True(outcome.IsSuccess);
        Assert.Equal(
            [
                TransitionObservationKind.Started, TransitionObservationKind.Committed,
                TransitionObservationKind.Completed, TransitionObservationKind.Outcome
            ],
            observer.Observations.Select(o => o.Kind));

        var completed = observer.Observations.Single(o => o.Kind == TransitionObservationKind.Completed);
        Assert.NotNull(completed.ActiveStatePath);
        Assert.Equal([HierarchyState.Reviewing, HierarchyState.LegalReview],
            completed.ActiveStatePath!.StatesRootToLeaf);
    }
}