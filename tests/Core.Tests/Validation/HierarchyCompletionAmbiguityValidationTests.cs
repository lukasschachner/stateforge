using Core.Tests.Hierarchy;
using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Validation;

namespace Core.Tests.Validation;

public class HierarchyCompletionAmbiguityValidationTests
{
    [Fact]
    public void CompletionAmbiguityUsesTransitionAmbiguityRulesInCurrentModel()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing)
                .InitialChild(HierarchyState.AuthorReview)
                .On<Hierarchy.Cancel>().GoTo(HierarchyState.Rejected)
                .On<Hierarchy.Cancel>().GoTo(HierarchyState.Published);
            builder.State(HierarchyState.AuthorReview).Terminal();
            builder.State(HierarchyState.Rejected).Terminal();
            builder.State(HierarchyState.Published).Terminal();
        });

        var validation = definition.Validate();

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, f => f.Code == HierarchyValidationCodes.AmbiguousTransition);
        Assert.DoesNotContain(validation.Findings, f => f.Code == HierarchyValidationCodes.AmbiguousCompletion);
    }
}