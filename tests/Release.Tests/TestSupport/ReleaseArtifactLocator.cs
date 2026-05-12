namespace Release.Tests.TestSupport;

internal static class ReleaseArtifactLocator
{
    public static string? FindPackage(PackableProject project, string extension = ".nupkg")
    {
        var directory = ProjectPaths.ArtifactsPackageDirectory;
        if (!Directory.Exists(directory)) return null;
        return Directory.EnumerateFiles(directory, project.PackageId + ".*" + extension)
            .Where(path => !path.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    public static IReadOnlyList<string> AllPackages(string extension = ".nupkg")
    {
        var directory = ProjectPaths.ArtifactsPackageDirectory;
        if (!Directory.Exists(directory)) return [];
        return Directory.EnumerateFiles(directory, "*" + extension).OrderBy(p => p, StringComparer.Ordinal).ToArray();
    }
}