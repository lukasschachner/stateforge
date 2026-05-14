using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class PackageMetadataTests
{
    [Fact]
    public void ExactlyExpectedPackableProjectsAreInCatalog()
    {
        Assert.Equal(
        [
            "Core", "DependencyInjection", "Logging", "SourceGenerators", "Persistence", "OpenTelemetry", "Visualization.Mermaid",
            "Visualization.Graphviz", "Visualization.PlantUML"
        ], PackableProject.All.Select(p => p.Name).ToArray());
    }

    [Theory]
    [MemberData(nameof(PackableProjects))]
    public void PackableProjectsHaveCompleteDistinctMetadata(PackableProject project)
    {
        var doc = ProjectFileAssertions.LoadProject(project.ProjectPath);
        Assert.Equal(project.PackageId, ProjectFileAssertions.RequiredProperty(doc, "PackageId"));

        var title = ProjectFileAssertions.RequiredProperty(doc, "Title");
        var expectedTitleFragment = project.Name switch
        {
            "SourceGenerators" => "Source Generators",
            var name when name.StartsWith("Visualization.", StringComparison.Ordinal) => name
                .Split('.', StringSplitOptions.RemoveEmptyEntries).Last(),
            _ => project.Name
        };

        Assert.Contains(expectedTitleFragment, title, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("state-machine", ProjectFileAssertions.RequiredProperty(doc, "PackageTags"),
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(project.Responsibility.Split(' ')[0],
            ProjectFileAssertions.RequiredProperty(doc, "Description"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SharedPackageMetadataExists()
    {
        foreach (var property in new[]
                 {
                     "PackageVersion", "Authors", "RepositoryUrl", "RepositoryType", "PackageLicenseFile",
                     "PackageReadmeFile", "GenerateDocumentationFile", "IncludeSymbols", "SymbolPackageFormat",
                     "IncludeSource"
                 }) ProjectFileAssertions.SharedPropertyExists(property);
    }

    public static IEnumerable<object[]> PackableProjects()
    {
        return PackableProject.All.Select(p => new object[] { p });
    }
}