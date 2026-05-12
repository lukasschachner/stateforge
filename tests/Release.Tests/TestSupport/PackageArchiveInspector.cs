using System.IO.Compression;
using System.Xml.Linq;

namespace Release.Tests.TestSupport;

internal sealed record PackageArchiveInventory(
    string PackagePath,
    string PackageId,
    string Version,
    IReadOnlyDictionary<string, string> Metadata,
    IReadOnlyList<string> Files,
    IReadOnlyList<string> Dependencies)
{
    public bool HasReadme => Files.Any(f => f.Equals("README.md", StringComparison.OrdinalIgnoreCase));
    public bool HasLicense => Files.Any(f => f.Equals("LICENSE", StringComparison.OrdinalIgnoreCase));
    public bool HasXmlDocumentation => Files.Any(f => f.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

    public bool HasAnalyzerAsset => Files.Any(f =>
        f.StartsWith("analyzers/dotnet/cs/", StringComparison.OrdinalIgnoreCase) &&
        f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase));
}

internal static class PackageArchiveInspector
{
    public static PackageArchiveInventory Inspect(string packagePath)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var files = archive.Entries.Select(e => e.FullName.Replace('\\', '/')).OrderBy(f => f, StringComparer.Ordinal)
            .ToArray();
        var nuspecEntry =
            archive.Entries.Single(e => e.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
        using var nuspecStream = nuspecEntry.Open();
        var doc = XDocument.Load(nuspecStream);
        var ns = doc.Root?.Name.Namespace ?? XNamespace.None;
        var metadataElement = doc.Descendants(ns + "metadata").Single();
        var metadata = metadataElement.Elements()
            .Where(e => !e.HasElements)
            .GroupBy(e => e.Name.LocalName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Value.Trim(), StringComparer.OrdinalIgnoreCase);
        var dependencies = metadataElement.Descendants(ns + "dependency")
            .Select(e => e.Attribute("id")?.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!)
            .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return new PackageArchiveInventory(
            packagePath,
            metadata.TryGetValue("id", out var id) ? id : string.Empty,
            metadata.TryGetValue("version", out var version) ? version : string.Empty,
            metadata,
            files,
            dependencies);
    }
}