using Core.Tests.Hierarchy;
using StateMachineLibrary.Core.Introspection;

namespace Core.Tests.Introspection;

public class HierarchyGraphRelationshipTests
{
    [Fact]
    public void GraphExportIncludesParentChildRelationshipsAndInitialChildMarkers()
    {
        var graph = Assert.IsType<DefinitionGraph<HierarchyState, HierarchyEvent>>(HierarchyTestDomain
            .CreateReviewMachine().ExportGraph().Graph);

        Assert.NotEmpty(graph.ParentChildRelationships);
        Assert.Contains(graph.ParentChildRelationships,
            r => r.ParentState == HierarchyState.Reviewing && r.ChildState == HierarchyState.AuthorReview);
        Assert.Contains(graph.ParentChildRelationships,
            r => r.ParentState == HierarchyState.Reviewing && r.ChildState == HierarchyState.LegalReview);
        Assert.Contains(graph.ParentChildRelationships,
            r => r.ParentState == HierarchyState.Reviewing && r.ChildState == HierarchyState.Approved);

        var marker = Assert.Single(graph.InitialChildMarkers, m => m.CompositeState == HierarchyState.Reviewing);
        Assert.Equal(HierarchyState.AuthorReview, marker.InitialChildState);
        Assert.Equal(HierarchyState.AuthorReview, marker.ResolvedInitialLeafState);
        Assert.Equal([HierarchyState.Reviewing, HierarchyState.AuthorReview], marker.ResolvedPath);
    }
}