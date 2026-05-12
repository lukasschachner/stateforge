using Core.Tests.Hierarchy;
using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Validation;

namespace Core.Tests.Validation;

public class HierarchyInitialChildValidationTests
{
    [Fact]
    public void CompositeWithoutInitialChildIsRejected()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing);
            builder.State(HierarchyState.AuthorReview).ChildOf(HierarchyState.Reviewing);
        });

        var validation = definition.Validate();

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, f => f.Code == HierarchyValidationCodes.MissingInitialChild);
    }

    [Fact]
    public void InitialChildMustRemainDirectChildOfComposite()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing).InitialChild(HierarchyState.AuthorReview);
            builder.State(HierarchyState.OtherComposite).InitialChild(HierarchyState.OtherLeaf);
            builder.State(HierarchyState.AuthorReview).ChildOf(HierarchyState.OtherComposite);
        });

        var validation = definition.Validate();

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, f => f.Code == HierarchyValidationCodes.InvalidInitialChild);
    }
}