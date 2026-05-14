using StateForge.Core.Tests.Hierarchy;
using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Execution;

public class HierarchyNestedCompletionTests
{
    [Fact]
    public async Task TerminalChildDoesNotImplicitlyCompleteWholeMachine()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing)
                .InitialChild(HierarchyState.LegalReview)
                .On<Hierarchy.Cancel>().GoTo(HierarchyState.Rejected);

            builder.State(HierarchyState.LegalReview)
                .On<Approve>().GoTo(HierarchyState.Approved);

            builder.State(HierarchyState.Approved)
                .ChildOf(HierarchyState.Reviewing)
                .Terminal();

            builder.State(HierarchyState.Rejected).Terminal();
        });

        var reachedTerminalChild = await definition.ApplyAsync(HierarchyState.LegalReview, new Approve());
        var parentFallbackAfterTerminalChild =
            await definition.ApplyAsync(HierarchyState.Approved, new Hierarchy.Cancel());

        Assert.True(reachedTerminalChild.IsSuccess);
        Assert.Equal(HierarchyState.Approved, reachedTerminalChild.ResultingState);
        Assert.Equal([HierarchyState.Reviewing, HierarchyState.Approved],
            reachedTerminalChild.ActiveStatePath.StatesRootToLeaf);

        Assert.True(parentFallbackAfterTerminalChild.IsSuccess);
        Assert.Equal(HierarchyState.Rejected, parentFallbackAfterTerminalChild.ResultingState);
    }
}