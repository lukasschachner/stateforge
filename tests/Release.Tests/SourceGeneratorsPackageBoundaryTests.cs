using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class SourceGeneratorsPackageBoundaryTests
{
    [Fact]
    public void SourceGeneratorBoundaryForbidsRuntimeVisualizationDependencies()
    {
        var rules = PackageBoundaryRules.Load();
        var rule = Assert.Contains("StateMachineLibrary.SourceGenerators", rules);
        Assert.Contains(rule.ForbiddenDependencyPatterns,
            pattern => pattern.Contains("StateMachineLibrary.Visualization", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SourceGeneratorProjectDoesNotReferenceVisualizationProjects()
    {
        var project = ProjectFileAssertions.LoadProject("src/SourceGenerators/SourceGenerators.csproj");
        var references = project.Descendants("ProjectReference")
            .Select(e => e.Attribute("Include")?.Value ?? string.Empty)
            .ToArray();
        Assert.DoesNotContain(references,
            reference => reference.Contains("Visualization", StringComparison.OrdinalIgnoreCase));
    }
}
