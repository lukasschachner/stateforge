using Core.Tests.Hierarchy;
using StateMachineLibrary.Core.Introspection;

namespace Core.Tests.Introspection;

public class HierarchyGraphEdgeTests
{
    [Fact]
    public void GraphEdgesIncludeHierarchySourceAndTargetResolutionMetadata()
    {
        var graph = Assert.IsType<DefinitionGraph<HierarchyState, HierarchyEvent>>(HierarchyTestDomain
            .CreateReviewMachine().ExportGraph().Graph);

        var toComposite = graph.Edges.Single(e =>
            e.SourceState == HierarchyState.Draft && e.TargetState == HierarchyState.Reviewing);
        Assert.False(toComposite.SourceIsComposite);
        Assert.True(toComposite.TargetIsComposite);
        Assert.True(toComposite.TargetResolvesThroughInitialChild);
        Assert.Equal(HierarchyState.AuthorReview, toComposite.ResolvedTargetLeafState);

        var fallback = graph.Edges.Single(e =>
            e.SourceState == HierarchyState.Reviewing && e.TargetState == HierarchyState.Rejected);
        Assert.True(fallback.SourceIsComposite);
        Assert.False(string.IsNullOrWhiteSpace(fallback.HierarchyRelationship));
    }
}