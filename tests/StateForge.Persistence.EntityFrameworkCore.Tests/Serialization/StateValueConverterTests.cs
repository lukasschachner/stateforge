using StateForge.Persistence.EntityFrameworkCore.Serialization;
using StateForge.Persistence.EntityFrameworkCore.Tests.TestSupport;

namespace StateForge.Persistence.EntityFrameworkCore.Tests.Serialization;

public sealed class StateValueConverterTests
{
    [Fact]
    public void DefaultEnumConverter_RoundTripsValue()
    {
        var converter = StateValueConverters.CreateDefault<EfOrderState>();

        var stored = converter.ConvertToStorage(EfOrderState.Submitted);
        var value = converter.ConvertFromStorage(stored);

        Assert.Equal(EfOrderState.Submitted, value);
    }
}
