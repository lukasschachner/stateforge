namespace StateForge.DependencyInjection.Tests.Persistence;

public sealed class PersistenceProviderNeutralityTests
{
    [Fact]
    public void DependencyInjectionProjectDoesNotReferenceConcretePersistenceProviders()
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/StateForge.DependencyInjection/StateForge.DependencyInjection.csproj"));
        var content = File.ReadAllText(path);
        Assert.DoesNotContain("EntityFramework", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Npgsql", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqlClient", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Mongo", content, StringComparison.OrdinalIgnoreCase);
    }
}
