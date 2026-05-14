using StateForge.Core.Definitions;

namespace StateForge.Core.Tests.Hierarchy;

public enum HierarchyState
{
    Draft,
    Reviewing,
    AuthorReview,
    LegalReview,
    Approved,
    Published,
    Rejected,
    OtherComposite,
    OtherLeaf
}

public abstract record HierarchyEvent;

public sealed record Submit : HierarchyEvent;

public sealed record Approve : HierarchyEvent;

public sealed record Cancel : HierarchyEvent;

public sealed record Reset : HierarchyEvent;

public static class HierarchyTestDomain
{
    public static StateMachineDefinition<HierarchyState, HierarchyEvent> CreateReviewMachine(List<string>? log = null)
    {
        return StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Draft)
                .On<Submit>().GoTo(HierarchyState.Reviewing);
            builder.State(HierarchyState.Reviewing)
                .InitialChild(HierarchyState.AuthorReview)
                .On<Cancel>().GoTo(HierarchyState.Rejected);
            builder.State(HierarchyState.AuthorReview)
                .OnEntry(_ => log?.Add("entry AuthorReview"))
                .OnExit(_ => log?.Add("exit AuthorReview"))
                .On<Submit>().GoTo(HierarchyState.LegalReview);
            builder.State(HierarchyState.LegalReview)
                .ChildOf(HierarchyState.Reviewing)
                .OnEntry(_ => log?.Add("entry LegalReview"))
                .OnExit(_ => log?.Add("exit LegalReview"))
                .On<Approve>().GoTo(HierarchyState.Approved);
            builder.State(HierarchyState.Approved).ChildOf(HierarchyState.Reviewing).Terminal();
            builder.State(HierarchyState.Published).Terminal();
            builder.State(HierarchyState.Rejected).Terminal();
        });
    }
}