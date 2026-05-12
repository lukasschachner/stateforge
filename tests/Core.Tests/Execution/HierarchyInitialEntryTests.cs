using Core.Tests.Hierarchy;
using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Execution;

public class HierarchyInitialEntryTests
{
    [Fact]
    public async Task CompositeTargetsResolveThroughMultiLevelInitialChildChains()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Draft)
                .On<Submit>().GoTo(HierarchyState.Reviewing);

            builder.State(HierarchyState.Reviewing)
                .InitialChild(HierarchyState.OtherComposite);

            builder.State(HierarchyState.OtherComposite)
                .ChildOf(HierarchyState.Reviewing)
                .InitialChild(HierarchyState.AuthorReview);

            builder.State(HierarchyState.AuthorReview).ChildOf(HierarchyState.OtherComposite);
            builder.State(HierarchyState.Rejected).Terminal();
        });

        var outcome = await definition.ApplyAsync(HierarchyState.Draft, new Submit());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(HierarchyState.AuthorReview, outcome.ResultingState);
        HierarchyAssertions.ActivePathIs(outcome.ActiveStatePath, HierarchyState.Reviewing,
            HierarchyState.OtherComposite, HierarchyState.AuthorReview);
    }
}