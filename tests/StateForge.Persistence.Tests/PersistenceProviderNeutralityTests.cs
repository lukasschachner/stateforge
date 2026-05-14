using System.Xml.Linq;

namespace StateForge.Persistence.Tests;

public class PersistenceProviderNeutralityTests
{
    [Fact]
    public void PersistenceProjectContainsNoProviderOrHostingPackageReferences()
    {
        var projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../src/StateForge.Persistence/StateForge.Persistence.csproj"));
        var doc = XDocument.Load(projectPath);

        var packageReferences = doc
            .Descendants("PackageReference")
            .Select(x => x.Attribute("Include")?.Value)
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToArray();

        Assert.DoesNotContain(packageReferences,
            p => p.Contains("EntityFramework", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(packageReferences, p => p.Contains("Npgsql", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(packageReferences, p => p.Contains("SqlClient", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(packageReferences, p => p.Contains("Hosting", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(packageReferences,
            p => p.Contains("DependencyInjection", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(packageReferences, p => p.Contains("Logging", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(packageReferences, p => p.Contains("Json", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PersistenceSourceUsesNoProviderSpecificNamespaces()
    {
        var sourceRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/StateForge.Persistence"));
        var files = Directory.GetFiles(sourceRoot, "*.cs", SearchOption.AllDirectories);
        var content = string.Join("\n", files.Select(File.ReadAllText));

        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Npgsql", content, StringComparison.Ordinal);
        Assert.DoesNotContain("System.Data.SqlClient", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Microsoft.Extensions.Hosting", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Microsoft.Extensions.DependencyInjection", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Microsoft.Extensions.Logging", content, StringComparison.Ordinal);
        Assert.DoesNotContain("System.Text.Json", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Newtonsoft.Json", content, StringComparison.Ordinal);
    }
}