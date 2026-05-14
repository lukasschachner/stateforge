namespace StateForge.Persistence.EntityFrameworkCore.Tests.Security;

public sealed class SqlConstructionBoundaryTests
{
    [Fact]
    public void AdapterStore_DoesNotUseRawSqlConstructionApis()
    {
        var source = File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "src",
            "StateForge.Persistence.EntityFrameworkCore",
            "Storage",
            "EntityFrameworkCoreSnapshotStore.cs"));

        Assert.DoesNotContain("FromSqlRaw", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ExecuteSqlRaw", source, StringComparison.Ordinal);
        Assert.DoesNotContain("FromSqlInterpolated", source, StringComparison.Ordinal);
    }
}
