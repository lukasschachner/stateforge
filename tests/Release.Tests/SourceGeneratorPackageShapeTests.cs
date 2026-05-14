using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class SourceGeneratorPackageShapeTests
{
    [Fact]
    public void SourceGeneratorRoslynDependencyIsPrivateAndPackagedAsAnalyzer()
    {
        var project = ProjectFileAssertions.LoadProject("src/StateForge.SourceGenerators/StateForge.SourceGenerators.csproj");
        Assert.Equal("Analyzer", ProjectFileAssertions.RequiredProperty(project, "PackageType"));
        Assert.Equal("false", ProjectFileAssertions.RequiredProperty(project, "IncludeBuildOutput"));
        var roslyn = project.Descendants("PackageReference")
            .Single(e => e.Attribute("Include")?.Value == "Microsoft.CodeAnalysis.CSharp");
        Assert.Equal("all", roslyn.Attribute("PrivateAssets")?.Value);
        Assert.Contains(project.Descendants("None"),
            e => (e.Attribute("PackagePath")?.Value ?? string.Empty).Contains("analyzers/dotnet/cs",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SourceGeneratorPackageContainsAnalyzerAssetWhenArtifactExists()
    {
        var project = PackableProject.All.Single(p => p.Name == "SourceGenerators");
        var package = ReleaseArtifactLocator.FindPackage(project);
        if (package is null) return;
        var inventory = PackageArchiveInspector.Inspect(package);
        Assert.True(inventory.HasAnalyzerAsset);
        Assert.DoesNotContain("Microsoft.CodeAnalysis.CSharp", inventory.Dependencies,
            StringComparer.OrdinalIgnoreCase);
    }
}