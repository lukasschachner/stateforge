using StateForge.Core.Tests.Hierarchy;
using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Execution;

public class HierarchyCrossBranchTransitionTests
{
    [Fact]
    public async Task CrossBranchTransitionResolvesCompositeTargetToBranchLeaf()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing)
                .InitialChild(HierarchyState.AuthorReview);

            builder.State(HierarchyState.AuthorReview)
                .On<Reset>().GoTo(HierarchyState.OtherComposite);

            builder.State(HierarchyState.OtherComposite)
                .ChildOf(HierarchyState.Reviewing)
                .InitialChild(HierarchyState.OtherLeaf);

            builder.State(HierarchyState.OtherLeaf).ChildOf(HierarchyState.OtherComposite);
        });

        var outcome = await definition.ApplyAsync(HierarchyState.AuthorReview, new Reset());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(HierarchyState.OtherLeaf, outcome.ResultingState);
        HierarchyAssertions.ActivePathIs(outcome.ActiveStatePath, HierarchyState.Reviewing,
            HierarchyState.OtherComposite, HierarchyState.OtherLeaf);
    }
}