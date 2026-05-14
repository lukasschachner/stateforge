using StateForge.DependencyInjection.Registration;

namespace StateForge.DependencyInjection.Tests.Persistence;

public sealed class PersistenceCoordinationFailureTests
{
    [Fact]
    public void NullCoordinatorIsRejected()
    {
        Assert.Throws<ArgumentNullException>(() => new PersistenceCoordinationOptions<int, int>(null!));
    }
}
