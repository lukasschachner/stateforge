using Core.Tests.Hierarchy;
using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Diagnostics;
using StateMachineLibrary.Core.Validation;

namespace Core.Tests.Validation;

public class HierarchyReferenceValidationTests
{
    [Fact]
    public void SelfParentRelationshipIsRejected()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing).ChildOf(HierarchyState.Reviewing);
        });

        var validation = definition.Validate();

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, f => f.Code == HierarchyValidationCodes.SelfParent);
    }

    [Fact]
    public void Invalid_transition_target_exposes_invalid_post_shape_conflict_diagnostic()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.State("A").On("go").GoTo("Missing");
        });

        var validation = definition.Validate();

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, finding => finding.Code == "TRANSITION002");
        Assert.Contains(validation.ConflictDiagnostics,
            diagnostic => diagnostic.Kind == TransitionConflictKind.InvalidPostShape &&
                          diagnostic.ValidationCode == "TRANSITION002");
    }

    [Fact]
    public void ReparentedInitialChildIsRejectedAsInvalidDirectChildReference()
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