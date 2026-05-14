using StateForge.Core.Tests.Hierarchy;
using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Execution;

public class HierarchyEntryExitOrderingTests
{
    [Fact]
    public async Task CrossBranchTransitionExitsToLcaThenEntersTargetBranch()
    {
        var log = new List<string>();

        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing).InitialChild(HierarchyState.AuthorReview);
            builder.State(HierarchyState.AuthorReview)
                .OnExit(_ => log.Add("exit AuthorReview"))
                .On<Reset>().GoTo(HierarchyState.OtherComposite);

            builder.State(HierarchyState.OtherComposite)
                .ChildOf(HierarchyState.Reviewing)
                .InitialChild(HierarchyState.OtherLeaf)
                .OnEntry(_ => log.Add("entry OtherComposite"));

            builder.State(HierarchyState.OtherLeaf)
                .ChildOf(HierarchyState.OtherComposite)
                .OnEntry(_ => log.Add("entry OtherLeaf"));
        });

        var outcome = await definition.ApplyAsync(HierarchyState.AuthorReview, new Reset());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(["exit AuthorReview", "entry OtherComposite", "entry OtherLeaf"], log);
    }
}