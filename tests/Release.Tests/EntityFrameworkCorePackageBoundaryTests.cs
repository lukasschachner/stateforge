using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class EntityFrameworkCorePackageBoundaryTests
{
    [Fact]
    public void EntityFrameworkCorePackageDependsOnPersistenceCoreAndEfCoreOnly()
    {
        var project = PackableProject.All.Single(p => p.Name == "Persistence.EntityFrameworkCore");
        var references = ProjectFileAssertions.ProjectReferences(project.ProjectPath);

        Assert.Equal([
            "../StateForge.Core/StateForge.Core.csproj",
            "../StateForge.Persistence/StateForge.Persistence.csproj"
        ], references);

        var packageReferences = ProjectFileAssertions.PackageReferences(project.ProjectPath);
        Assert.Equal(["Microsoft.EntityFrameworkCore"], packageReferences);

        var rule = PackageBoundaryRules.Load()[project.PackageId];
        PackageBoundaryRules.AssertNoForbiddenDependencies(packageReferences, rule);
    }

    [Fact]
    public void EntityFrameworkCorePackageHasNoForbiddenFilesWhenArtifactExists()
    {
        var project = PackableProject.All.Single(p => p.Name == "Persistence.EntityFrameworkCore");
        var package = ReleaseArtifactLocator.FindPackage(project);
        if (package is null) return;

        PackageBoundaryRules.AssertNoForbiddenFiles(PackageArchiveInspector.Inspect(package).Files,
            PackageBoundaryRules.Load()[project.PackageId]);
    }
}
