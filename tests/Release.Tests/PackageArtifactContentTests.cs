using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class PackageArtifactContentTests
{
    [Fact]
    public void ArtifactDirectoryContainsOnlyExpectedPackageIdsWhenArtifactsExist()
    {
        var packages = ReleaseArtifactLocator.AllPackages();
        if (packages.Count == 0) return;

        var ids = packages.Select(PackageArchiveInspector.Inspect).Select(i => i.PackageId)
            .OrderBy(i => i, StringComparer.Ordinal).Distinct().ToArray();
        var expected = PackableProject.All.Select(p => p.PackageId).OrderBy(i => i, StringComparer.Ordinal).ToArray();

        foreach (var id in ids) Assert.Contains(id, expected);
    }

    [Theory]
    [MemberData(nameof(PackableProjects))]
    public void PackageInventoryExcludesRepositoryTestSpecCiAndLocalFiles(PackableProject project)
    {
        var package = ReleaseArtifactLocator.FindPackage(project);
        if (package is null) return;
        PackageBoundaryRules.AssertNoForbiddenFiles(PackageArchiveInspector.Inspect(package).Files,
            PackageBoundaryRules.Load()[project.PackageId]);
    }

    public static IEnumerable<object[]> PackableProjects()
    {
        return PackableProject.All.Select(p => new object[] { p });
    }
}