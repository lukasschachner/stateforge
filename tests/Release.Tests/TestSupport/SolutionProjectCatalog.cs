using System.Text.RegularExpressions;

namespace Release.Tests.TestSupport;

internal sealed record SolutionProject(string Name, string RelativePath);

internal static class SolutionProjectCatalog
{
    private static readonly Regex ProjectLine = new(
        "^Project\\(\"[^\"]+\"\\) = \"(?<name>[^\"]+)\", \"(?<path>[^\"]+)\", \"[^\"]+\"",
        RegexOptions.Compiled);

    public static IReadOnlyList<SolutionProject> Load()
    {
        var projects = new List<SolutionProject>();
        foreach (var line in File.ReadLines(ProjectPaths.SolutionPath))
        {
            var match = ProjectLine.Match(line);
            if (!match.Success) continue;
            var path = match.Groups["path"].Value.Replace('\\', '/');
            if (path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                projects.Add(new SolutionProject(match.Groups["name"].Value, path));
        }

        return projects.OrderBy(p => p.RelativePath, StringComparer.Ordinal).ToArray();
    }
}