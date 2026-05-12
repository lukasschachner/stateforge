using System.Text.Json;

namespace Release.Tests.TestSupport;

internal sealed record PackageBoundaryRule(
    string PackageId,
    string[] AllowedDependencies,
    string[] ForbiddenDependencyPatterns,
    string[] ForbiddenFilePatterns,
    bool AnalyzerAssetsRequired);

internal static class PackageBoundaryRules
{
    public static string[] CommonForbiddenFiles { get; } =
        [".git/", ".github/", ".idea/", "obj/", "tests/", "samples/", "specs/", "StateMachineLibrary.sln"];

    public static IReadOnlyDictionary<string, PackageBoundaryRule> Load()
    {
        var path = ProjectPaths.FullPath("eng/package-boundaries.json");
        if (File.Exists(path))
        {
            using var stream = File.OpenRead(path);
            var doc = JsonDocument.Parse(stream);
            return doc.RootElement.GetProperty("packages").EnumerateArray()
                .Select(e => new PackageBoundaryRule(
                    e.GetProperty("packageId").GetString()!,
                    e.GetProperty("allowedDependencies").EnumerateArray().Select(x => x.GetString()!).ToArray(),
                    e.GetProperty("forbiddenDependencyPatterns").EnumerateArray().Select(x => x.GetString()!).ToArray(),
                    e.GetProperty("forbiddenFilePatterns").EnumerateArray().Select(x => x.GetString()!).ToArray(),
                    e.GetProperty("analyzerAssetsRequired").GetBoolean()))
                .ToDictionary(r => r.PackageId, StringComparer.OrdinalIgnoreCase);
        }

        return PackableProject.All.ToDictionary(
            p => p.PackageId,
            p => new PackageBoundaryRule(p.PackageId, [], ForbiddenDependenciesFor(p.Name), CommonForbiddenFiles,
                p.AnalyzerPackage),
            StringComparer.OrdinalIgnoreCase);
    }

    public static string[] ForbiddenDependenciesFor(string projectName)
    {
        return projectName switch
        {
            "Core" =>
            [
                "Hosting", "Logging", "DependencyInjection", "System.Text.Json", "Newtonsoft", "EntityFramework", "Sql",
                "Npgsql", "Mongo", "Visualization", "Graphviz", "PlantUML", "SourceGenerators", "Persistence",
                "OpenTelemetry"
            ],
            "Persistence" =>
            [
                "Hosting", "Logging", "DependencyInjection", "System.Text.Json", "Newtonsoft", "EntityFramework", "Sql",
                "Npgsql", "Mongo", "Visualization", "Graphviz", "PlantUML"
            ],
            "OpenTelemetry" =>
            [
                "Hosting", "Logging", "DependencyInjection", "OpenTelemetry.Exporter", "EntityFramework", "Sql",
                "Npgsql", "Mongo", "Visualization", "Graphviz", "PlantUML"
            ],
            "SourceGenerators" =>
            [
                "StateMachineLibrary.Persistence", "StateMachineLibrary.OpenTelemetry",
                "StateMachineLibrary.Visualization"
            ],
            "Visualization.Mermaid" =>
            [
                "Hosting", "Logging", "DependencyInjection", "OpenTelemetry", "Persistence", "SourceGenerators",
                "Visualization.Graphviz", "Visualization.PlantUML"
            ],
            "Visualization.Graphviz" =>
            [
                "Hosting", "Logging", "DependencyInjection", "OpenTelemetry", "Persistence", "SourceGenerators",
                "Visualization.Mermaid", "Visualization.PlantUML"
            ],
            "Visualization.PlantUML" =>
            [
                "Hosting", "Logging", "DependencyInjection", "OpenTelemetry", "Persistence", "SourceGenerators",
                "Visualization.Mermaid", "Visualization.Graphviz"
            ],
            _ => []
        };
    }

    public static void AssertNoForbiddenDependencies(IEnumerable<string> dependencies, PackageBoundaryRule rule)
    {
        var offenders = dependencies.Where(dep =>
                rule.ForbiddenDependencyPatterns.Any(pattern =>
                    dep.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        Assert.True(offenders.Length == 0,
            $"Forbidden dependencies for {rule.PackageId}: {string.Join(", ", offenders)}");
    }

    public static void AssertNoForbiddenFiles(IEnumerable<string> files, PackageBoundaryRule rule)
    {
        var offenders = files.Where(file =>
                rule.ForbiddenFilePatterns.Any(pattern => file.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        Assert.True(offenders.Length == 0,
            $"Forbidden package files for {rule.PackageId}: {string.Join(", ", offenders)}");
    }
}