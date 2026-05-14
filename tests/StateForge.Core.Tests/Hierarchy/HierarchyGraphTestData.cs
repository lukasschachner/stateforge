namespace StateForge.Core.Tests.Hierarchy;

public static class HierarchyGraphTestData
{
    public static string[] ReviewHierarchyLabels =>
    [
        nameof(HierarchyState.Reviewing),
        nameof(HierarchyState.AuthorReview),
        nameof(HierarchyState.LegalReview),
        nameof(HierarchyState.Approved)
    ];
}