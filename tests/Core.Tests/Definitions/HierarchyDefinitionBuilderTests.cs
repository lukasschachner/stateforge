using Core.Tests.Hierarchy;
using StateMachineLibrary.Core.Definitions;

namespace Core.Tests.Definitions;

public class HierarchyDefinitionBuilderTests
{
    [Fact]
    public void BuilderSupportsParentChildAndInitialChildDeclarations()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing).InitialChild(HierarchyState.AuthorReview);
            builder.State(HierarchyState.LegalReview).ChildOf(HierarchyState.Reviewing);
            builder.State(HierarchyState.Rejected).Terminal();
        });

        Assert.True(definition.HasHierarchy);
        Assert.True(definition.TryGetParent(HierarchyState.AuthorReview, out var authorParent));
        Assert.Equal(HierarchyState.Reviewing, authorParent);
        Assert.True(definition.TryGetParent(HierarchyState.LegalReview, out var legalParent));
        Assert.Equal(HierarchyState.Reviewing, legalParent);
        Assert.True(definition.TryGetInitialChild(HierarchyState.Reviewing, out var initialChild));
        Assert.Equal(HierarchyState.AuthorReview, initialChild);
    }

    [Fact]
    public void ConvenienceCompositeAndChildApisBuildEquivalentRelationships()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.CompositeState(HierarchyState.Reviewing, HierarchyState.AuthorReview);
            builder.ChildState(HierarchyState.LegalReview, HierarchyState.Reviewing);
            builder.State(HierarchyState.Rejected).Terminal();
        });

        Assert.True(definition.IsCompositeState(HierarchyState.Reviewing));
        Assert.Equal(2, definition.GetChildren(HierarchyState.Reviewing).Count);
        Assert.Contains(definition.GetChildren(HierarchyState.Reviewing), c => c.Value == HierarchyState.AuthorReview);
        Assert.Contains(definition.GetChildren(HierarchyState.Reviewing), c => c.Value == HierarchyState.LegalReview);
    }
}