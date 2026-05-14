using StateForge.Core.Tests.Hierarchy;
using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Execution;

public class HierarchyParentFallbackTransitionTests
{
    [Fact]
    public async Task ParentTransitionHandlesUnhandledChildEvent()
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

        var outcome = await definition.ApplyAsync(HierarchyState.LegalReview, new Hierarchy.Cancel());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(HierarchyState.Rejected, outcome.ResultingState);
        HierarchyAssertions.ActivePathIs(outcome.ActiveStatePath, HierarchyState.Rejected);
    }
}