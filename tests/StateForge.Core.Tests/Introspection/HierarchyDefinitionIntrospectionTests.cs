using StateForge.Core.Tests.Hierarchy;

namespace StateForge.Core.Tests.Introspection;

public class HierarchyDefinitionIntrospectionTests
{
    [Fact]
    public void IntrospectionExposesHierarchyFlagsChildrenAndInitialChildMetadata()
    {
        var definition = HierarchyTestDomain.CreateReviewMachine();
        var introspection = definition.Introspect();

        Assert.True(introspection.HasHierarchy);

        var reviewing = introspection.DeclaredStates.Single(s => s.Value == HierarchyState.Reviewing);
        Assert.True(reviewing.HasInitialChild);
        Assert.Equal(HierarchyState.AuthorReview, reviewing.InitialChildState);

        var children = introspection.ChildrenOf(HierarchyState.Reviewing).Select(s => s.Value).ToArray();
        Assert.Contains(HierarchyState.AuthorReview, children);
        Assert.Contains(HierarchyState.LegalReview, children);
        Assert.Contains(HierarchyState.Approved, children);
    }
}