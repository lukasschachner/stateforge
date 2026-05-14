using System.Text.RegularExpressions;

namespace Release.Tests.TestSupport;

internal static class ReleaseArtifactLocator
{
    public static string? FindPackage(PackableProject project, string extension = ".nupkg")
    {
        var directory = ProjectPaths.ArtifactsPackageDirectory;
        if (!Directory.Exists(directory)) return null;

        var filenamePattern = new Regex(
            $"^{Regex.Escape(project.PackageId)}\\.[0-9].*{Regex.Escape(extension)}$",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        return Directory.EnumerateFiles(directory, "*" + extension)
            .Where(path => !path.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase))
            .Where(path => filenamePattern.IsMatch(Path.GetFileName(path)))
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