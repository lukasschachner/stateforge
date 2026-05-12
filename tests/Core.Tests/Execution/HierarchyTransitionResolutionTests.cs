using Core.Tests.Hierarchy;
using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Execution;

public class HierarchyTransitionResolutionTests
{
    [Fact]
    public async Task LeafTransitionTakesPrecedenceOverAncestorTransitionForSameEvent()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing)
                .InitialChild(HierarchyState.AuthorReview)
                .On<Submit>().GoTo(HierarchyState.Rejected);
            builder.State(HierarchyState.AuthorReview)
                .On<Submit>().GoTo(HierarchyState.LegalReview);
            builder.State(HierarchyState.LegalReview).ChildOf(HierarchyState.Reviewing);
            builder.State(HierarchyState.Rejected).Terminal();
        });

        var outcome = await definition.ApplyAsync(HierarchyState.AuthorReview, new Submit());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(HierarchyState.LegalReview, outcome.ResultingState);
        HierarchyAssertions.ActivePathIs(outcome.ActiveStatePath, HierarchyState.Reviewing, HierarchyState.LegalReview);
    }

    [Fact]
    public async Task DeniedHierarchyTransitionCarriesResolutionDiagnosticsMetadata()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing)
                .InitialChild(HierarchyState.AuthorReview)
                .On<Hierarchy.Cancel>().GoTo(HierarchyState.Rejected);
            builder.State(HierarchyState.AuthorReview);
            builder.State(HierarchyState.LegalReview).ChildOf(HierarchyState.Reviewing);
            builder.State(HierarchyState.Rejected).Terminal();
        });

        var denied = await definition.ApplyAsync(HierarchyState.LegalReview, new Hierarchy.Cancel());

        Assert.True(denied.IsSuccess);

        var guardedDefinition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing)
                .InitialChild(HierarchyState.AuthorReview)
                .On<Hierarchy.Cancel>().When(_ => false, "deny ancestor cancel").GoTo(HierarchyState.Rejected);
            builder.State(HierarchyState.AuthorReview);
            builder.State(HierarchyState.LegalReview).ChildOf(HierarchyState.Reviewing);
            builder.State(HierarchyState.Rejected).Terminal();
        });

        var deniedOutcome = await guardedDefinition.ApplyAsync(HierarchyState.LegalReview, new Hierarchy.Cancel());

        Assert.Equal(TransitionOutcomeCategory.Denied, deniedOutcome.Category);
        Assert.NotNull(deniedOutcome.Diagnostics.HierarchyMetadata);
    }
}