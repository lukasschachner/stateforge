using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class VisualizationPackageBoundaryTests
{
    [Theory]
    [InlineData("Visualization.Mermaid", "src/StateForge.Visualization.Mermaid/StateForge.Visualization.Mermaid.csproj")]
    [InlineData("Visualization.Graphviz", "src/StateForge.Visualization.Graphviz/StateForge.Visualization.Graphviz.csproj")]
    [InlineData("Visualization.PlantUML", "src/StateForge.Visualization.PlantUML/StateForge.Visualization.PlantUML.csproj")]
    public void VisualizationRenderersDependOnlyOnCoreAndNoForbiddenPackages(string name, string path)
    {
        var project = PackableProject.All.Single(p => p.Name == name);
        Assert.Equal(path, project.ProjectPath);

        var packageRefs = ProjectFileAssertions.PackageReferences(project.ProjectPath);
        var projectRefs = ProjectFileAssertions.ProjectReferences(project.ProjectPath);

        Assert.Empty(packageRefs);
        Assert.Equal(["../StateForge.Core/StateForge.Core.csproj"], projectRefs);

        var rule = PackageBoundaryRules.Load()[project.PackageId];
        PackageBoundaryRules.AssertNoForbiddenDependencies(packageRefs, rule);
    }

    [Theory]
    [MemberData(nameof(RendererProjects))]
    public void VisualizationPackagesHaveNoForbiddenFilesWhenArtifactsExist(PackableProject project)
    {
        var package = ReleaseArtifactLocator.FindPackage(project);
        if (package is null) return;
        PackageBoundaryRules.AssertNoForbiddenFiles(PackageArchiveInspector.Inspect(package).Files,
            PackageBoundaryRules.Load()[project.PackageId]);
    }

    public static IEnumerable<object[]> RendererProjects()
    {
        return PackableProject.All.Where(p => p.Name.StartsWith("Visualization.", StringComparison.Ordinal))
            .Select(p => new object[] { p });
    }
}