using StateForge.Core.Tests.Hierarchy;
using StateForge.Core.Introspection;

namespace StateForge.Core.Tests.Introspection;

public class HierarchyGraphNodeTests
{
    [Fact]
    public void GraphNodesIncludeCompositeLeafParentAndInitialChildMetadata()
    {
        var graph = Assert.IsType<DefinitionGraph<HierarchyState, HierarchyEvent>>(HierarchyTestDomain
            .CreateReviewMachine().ExportGraph().Graph);

        var reviewing = graph.Nodes.Single(n => n.State == HierarchyState.Reviewing);
        Assert.True(reviewing.IsComposite);
        Assert.False(reviewing.IsLeaf);
        Assert.True(reviewing.HasInitialChild);
        Assert.Equal(HierarchyState.AuthorReview, reviewing.InitialChildState);

        var legalReview = graph.Nodes.Single(n => n.State == HierarchyState.LegalReview);
        Assert.False(legalReview.IsComposite);
        Assert.True(legalReview.IsLeaf);
        Assert.True(legalReview.HasParent);
        Assert.Equal(HierarchyState.Reviewing, legalReview.ParentState);
    }
}