using StateForge.Core.Tests.Hierarchy;
using StateForge.Core.Definitions;
using StateForge.Core.Validation;

namespace StateForge.Core.Tests.Validation;

public class HierarchyCycleValidationTests
{
    [Fact]
    public void DirectParentCycleIsRejected()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing).ChildOf(HierarchyState.AuthorReview);
            builder.State(HierarchyState.AuthorReview).ChildOf(HierarchyState.Reviewing);
        });

        var validation = definition.Validate();

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, f => f.Code == HierarchyValidationCodes.Cycle);
    }

    [Fact]
    public void IndirectParentCycleIsRejected()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing).ChildOf(HierarchyState.AuthorReview);
            builder.State(HierarchyState.AuthorReview).ChildOf(HierarchyState.OtherComposite);
            builder.State(HierarchyState.OtherComposite).ChildOf(HierarchyState.Reviewing);
        });

        var validation = definition.Validate();

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, f => f.Code == HierarchyValidationCodes.Cycle);
    }
}