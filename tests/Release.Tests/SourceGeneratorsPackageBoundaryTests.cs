using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class SourceGeneratorsPackageBoundaryTests
{
    [Fact]
    public void SourceGeneratorBoundaryForbidsRuntimeVisualizationDependencies()
    {
        var rules = PackageBoundaryRules.Load();
        var rule = Assert.Contains("StateForge.SourceGenerators", rules);
        Assert.Contains(rule.ForbiddenDependencyPatterns,
            pattern => pattern.Contains("StateForge.Visualization", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SourceGeneratorProjectDoesNotReferenceVisualizationProjects()
    {
        var project = ProjectFileAssertions.LoadProject("src/StateForge.SourceGenerators/StateForge.SourceGenerators.csproj");
        var references = project.Descendants("ProjectReference")
            .Select(e => e.Attribute("Include")?.Value ?? string.Empty)
            .ToArray();
        Assert.DoesNotContain(references,
            reference => reference.Contains("Visualization", StringComparison.OrdinalIgnoreCase));
    }
}
