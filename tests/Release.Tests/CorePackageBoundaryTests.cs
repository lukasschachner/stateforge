using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class CorePackageBoundaryTests
{
    [Fact]
    public void CoreHasNoForbiddenProjectOrPackageDependencies()
    {
        var project = PackableProject.All.Single(p => p.Name == "Core");
        var rule = PackageBoundaryRules.Load()[project.PackageId];
        var packageReferences = ProjectFileAssertions.PackageReferences(project.ProjectPath);
        var projectReferences = ProjectFileAssertions.ProjectReferences(project.ProjectPath);

        Assert.Empty(packageReferences);
        Assert.Empty(projectReferences);
        Assert.DoesNotContain(packageReferences,
            dep => dep.Contains("Diagnostics", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(packageReferences,
            dep => dep.Contains("Visualization", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(projectReferences,
            dep => dep.Contains("Visualization", StringComparison.OrdinalIgnoreCase));
        PackageBoundaryRules.AssertNoForbiddenDependencies(packageReferences, rule);
    }

    [Fact]
    public void CorePackageHasNoForbiddenFilesWhenArtifactExists()
    {
        var project = PackableProject.All.Single(p => p.Name == "Core");
        var package = ReleaseArtifactLocator.FindPackage(project);
        if (package is null) return;
        PackageBoundaryRules.AssertNoForbiddenFiles(PackageArchiveInspector.Inspect(package).Files,
            PackageBoundaryRules.Load()[project.PackageId]);
    }
}