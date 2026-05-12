using Core.Tests.Hierarchy;
using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Validation;

namespace Core.Tests.Execution;

public class HierarchyRuntimeTests
{
    [Fact]
    public async Task FlatMachineKeepsSingleStatePathAndNoHierarchyFlag()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Draft)
                .On<Submit>().GoTo(HierarchyState.Published);
            builder.State(HierarchyState.Published).Terminal();
        });

        var outcome = await definition.ApplyAsync(HierarchyState.Draft, new Submit());

        Assert.False(definition.HasHierarchy);
        Assert.Equal(HierarchyState.Published, outcome.ResultingState);
        HierarchyAssertions.ActivePathIs(outcome.ActiveStatePath, HierarchyState.Published);
    }

    [Fact]
    public async Task EnteringCompositeTargetActivatesInitialLeafAndReportsPath()
    {
        var definition = HierarchyTestDomain.CreateReviewMachine();

        var outcome = await definition.ApplyAsync(HierarchyState.Draft, new Submit());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(HierarchyState.AuthorReview, outcome.ResultingState);
        HierarchyAssertions.ActivePathIs(outcome.ActiveStatePath, HierarchyState.Reviewing,
            HierarchyState.AuthorReview);
    }

    [Fact]
    public async Task LeafTransitionTakesPrecedenceAndParentFallbackHandlesUnhandledEvent()
    {
        var definition = HierarchyTestDomain.CreateReviewMachine();

        var leaf = await definition.ApplyAsync(HierarchyState.AuthorReview, new Submit());
        var fallback = await definition.ApplyAsync(HierarchyState.LegalReview, new Hierarchy.Cancel());

        Assert.Equal(HierarchyState.LegalReview, leaf.ResultingState);
        Assert.Equal(HierarchyState.Rejected, fallback.ResultingState);
    }

    [Fact]
    public async Task CrossBranchTransitionRunsExitThenEntryUsingLeastCommonAncestor()
    {
        var log = new List<string>();
        var definition = HierarchyTestDomain.CreateReviewMachine(log);

        await definition.ApplyAsync(HierarchyState.AuthorReview, new Submit());

        Assert.Equal(["exit AuthorReview", "entry LegalReview"], log);
    }

    [Fact]
    public void ValidationReportsMissingInitialChildForCompositeState()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing);
            builder.State(HierarchyState.AuthorReview).ChildOf(HierarchyState.Reviewing);
        });

        var validation = definition.Validate();

        Assert.False(validation.IsValid);
        HierarchyAssertions.ContainsFinding(validation, HierarchyValidationCodes.MissingInitialChild);
    }
}