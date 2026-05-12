using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class OpenTelemetryPackageBoundaryTests
{
    [Fact]
    public void OpenTelemetryDependsOnCoreOnlyAndNoForbiddenPackages()
    {
        var project = PackableProject.All.Single(p => p.Name == "OpenTelemetry");
        var rule = PackageBoundaryRules.Load()[project.PackageId];

        Assert.Empty(ProjectFileAssertions.PackageReferences(project.ProjectPath));
        Assert.Equal(["../Core/Core.csproj"], ProjectFileAssertions.ProjectReferences(project.ProjectPath));
        PackageBoundaryRules.AssertNoForbiddenDependencies(ProjectFileAssertions.PackageReferences(project.ProjectPath),
            rule);
    }

    [Fact]
    public void OpenTelemetryPackageHasNoForbiddenFilesWhenArtifactExists()
    {
        var project = PackableProject.All.Single(p => p.Name == "OpenTelemetry");
        var package = ReleaseArtifactLocator.FindPackage(project);
        if (package is null) return;
        PackageBoundaryRules.AssertNoForbiddenFiles(PackageArchiveInspector.Inspect(package).Files,
            PackageBoundaryRules.Load()[project.PackageId]);
    }
}