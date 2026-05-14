using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class PersistencePackageBoundaryTests
{
    [Fact]
    public void PersistenceDependsOnlyOnCoreProjectAndNoForbiddenPackages()
    {
        var project = PackableProject.All.Single(p => p.Name == "Persistence");
        Assert.Equal(["../StateForge.Core/StateForge.Core.csproj"], ProjectFileAssertions.ProjectReferences(project.ProjectPath));
        var rule = PackageBoundaryRules.Load()[project.PackageId];
        PackageBoundaryRules.AssertNoForbiddenDependencies(ProjectFileAssertions.PackageReferences(project.ProjectPath),
            rule);
    }

    [Fact]
    public void PersistencePackageHasNoForbiddenFilesWhenArtifactExists()
    {
        var project = PackableProject.All.Single(p => p.Name == "Persistence");
        var package = ReleaseArtifactLocator.FindPackage(project);
        if (package is null) return;
        PackageBoundaryRules.AssertNoForbiddenFiles(PackageArchiveInspector.Inspect(package).Files,
            PackageBoundaryRules.Load()[project.PackageId]);
    }
}