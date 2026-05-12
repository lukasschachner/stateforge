using System.Xml.Linq;

namespace Release.Tests.TestSupport;

internal static class ProjectFileAssertions
{
    public static XDocument LoadProject(string relativePath)
    {
        return XDocument.Load(ProjectPaths.FullPath(relativePath));
    }

    public static XDocument LoadDirectoryBuildProps()
    {
        return XDocument.Load(ProjectPaths.FullPath("Directory.Build.props"));
    }

    public static string? Property(XDocument doc, string name)
    {
        return doc.Descendants(name).Select(e => e.Value.Trim()).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
    }

    public static string RequiredProperty(XDocument doc, string name)
    {
        var value = Property(doc, name);
        Assert.False(string.IsNullOrWhiteSpace(value));
        return value!;
    }

    public static void ProjectHasProperty(string projectPath, string propertyName, string? expected = null)
    {
        var value = RequiredProperty(LoadProject(projectPath), propertyName);
        if (expected is not null) Assert.Equal(expected, value);
    }

    public static void SharedPropertyExists(string propertyName)
    {
        RequiredProperty(LoadDirectoryBuildProps(), propertyName);
    }

    public static IReadOnlyList<string> PackageReferences(string projectPath)
    {
        return LoadProject(projectPath)
            .Descendants("PackageReference")
            .Select(e => e.Attribute("Include")?.Value ?? e.Attribute("Update")?.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!)
            .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static IReadOnlyList<string> ProjectReferences(string projectPath)
    {
        return LoadProject(projectPath)
            .Descendants("ProjectReference")
            .Select(e => e.Attribute("Include")?.Value.Replace('\\', '/'))
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!)
            .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}