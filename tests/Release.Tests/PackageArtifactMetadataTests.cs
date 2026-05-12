using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class PackageArtifactMetadataTests
{
    [Theory]
    [MemberData(nameof(PackableProjects))]
    public void ProducedPackagesExposeRequiredMetadataWhenArtifactsExist(PackableProject project)
    {
        var package = ReleaseArtifactLocator.FindPackage(project);
        if (package is null) return;
        var inventory = PackageArchiveInspector.Inspect(package);
        Assert.Equal(project.PackageId, inventory.PackageId);
        foreach (var key in new[] { "id", "version", "authors", "description", "repository", "readme", "license" })
            Assert.True(inventory.Metadata.ContainsKey(key), $"Missing metadata {key} in {package}");
        Assert.True(inventory.HasReadme);
        Assert.True(inventory.HasLicense);
        Assert.True(inventory.HasXmlDocumentation || project.AnalyzerPackage);
    }

    public static IEnumerable<object[]> PackableProjects()
    {
        return PackableProject.All.Select(p => new object[] { p });
    }
}