using System.Xml.Linq;

namespace Persistence.Tests;

public class PersistenceProjectDependencyTests
{
    private static readonly string[] ForbiddenPackagePrefixes =
    [
        "Microsoft.EntityFrameworkCore",
        "Npgsql",
        "Microsoft.Data.SqlClient",
        "Dapper",
        "Microsoft.Extensions.Hosting",
        "Microsoft.Extensions.DependencyInjection",
        "Microsoft.Extensions.Logging",
        "System.Text.Json",
        "Newtonsoft.Json"
    ];

    [Fact]
    public void PersistenceProjectHasNoForbiddenPackageReferences()
    {
        var projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../src/Persistence/Persistence.csproj"));
        var doc = XDocument.Load(projectPath);

        var packageReferences = doc
            .Descendants("PackageReference")
            .Select(x => x.Attribute("Include")?.Value)
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToArray();

        foreach (var package in packageReferences)
            Assert.DoesNotContain(
                ForbiddenPackagePrefixes,
                forbidden => package.StartsWith(forbidden, StringComparison.OrdinalIgnoreCase));
    }
}