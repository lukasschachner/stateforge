using StateForge.Core.Tests.Completion;

namespace StateForge.Core.Tests.Introspection;

public sealed class CompletionPermittedEventsTests
{
    [Fact]
    public async Task Completion_transition_is_not_returned_as_permitted_user_event()
    {
        var definition = OrdinaryCompletionTestFixtures.CreateReviewingDefinition();

        var events = await definition.GetPermittedEventsAsync(CompletionState.AuthorReview);

        Assert.Contains(events, e => e.DisplayName == CompletionEvent.Approve.ToString());
        Assert.DoesNotContain(events, e => e.Identity == "completion");
    }
}
