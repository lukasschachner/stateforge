using StateForge.Core.Tests.Hierarchy;
using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Execution;

public class HierarchyCompositeTargetOrderingTests
{
    [Fact]
    public async Task EnteringCompositeTargetRunsParentEntryBeforeInitialChildEntry()
    {
        var log = new List<string>();

        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Draft).On<Submit>().GoTo(HierarchyState.Reviewing);
            builder.State(HierarchyState.Reviewing)
                .OnEntry(_ => log.Add("entry Reviewing"))
                .InitialChild(HierarchyState.AuthorReview);
            builder.State(HierarchyState.AuthorReview)
                .OnEntry(_ => log.Add("entry AuthorReview"));
        });

        var outcome = await definition.ApplyAsync(HierarchyState.Draft, new Submit());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(["entry Reviewing", "entry AuthorReview"], log);
    }
}