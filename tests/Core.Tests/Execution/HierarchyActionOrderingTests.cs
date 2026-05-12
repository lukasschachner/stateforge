using Core.Tests.Hierarchy;
using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Execution;

public class HierarchyActionOrderingTests
{
    [Fact]
    public async Task ExitTransitionAndEntryActionsComposeInDeterministicOrder()
    {
        var log = new List<string>();

        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing)
                .InitialChild(HierarchyState.AuthorReview);

            builder.State(HierarchyState.AuthorReview)
                .OnExit(_ => log.Add("exit AuthorReview"))
                .On<Submit>()
                .ExecuteAction(_ => log.Add("transition Submit"))
                .GoTo(HierarchyState.LegalReview);

            builder.State(HierarchyState.LegalReview)
                .ChildOf(HierarchyState.Reviewing)
                .OnEntry(_ => log.Add("entry LegalReview"));
        });

        var outcome = await definition.ApplyAsync(HierarchyState.AuthorReview, new Submit());

        Assert.True(outcome.IsSuccess);
        Assert.Equal(["exit AuthorReview", "transition Submit", "entry LegalReview"], log);
    }
}