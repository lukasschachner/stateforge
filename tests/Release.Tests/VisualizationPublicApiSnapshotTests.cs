using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class VisualizationPublicApiSnapshotTests
{
    [Theory]
    [MemberData(nameof(RendererProjects))]
    public void VisualizationRendererPublicApisMatchApprovedSnapshots(PackableProject project)
    {
        PublicApiSnapshotAssert.MatchesApproved(project);
    }

    public static IEnumerable<object[]> RendererProjects()
    {
        return PackableProject.All.Where(p => p.Name.StartsWith("Visualization.", StringComparison.Ordinal))
            .Select(p => new object[] { p });
    }
}