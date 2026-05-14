using StateForge.Persistence.EntityFrameworkCore.Serialization;
using StateForge.Persistence.Snapshots;

namespace StateForge.Persistence.EntityFrameworkCore.Tests.Serialization;

public sealed class SnapshotPayloadConverterTests
{
    [Fact]
    public void DefaultPayloadConverter_RoundTripsProperties()
    {
        var converter = StateValueConverters.CreateDefaultPayloadConverter();
        var bag = new PersistencePropertyBag(new[]
        {
            new KeyValuePair<string, object?>("key", "value")
        });

        var payload = converter.ConvertToStorage(bag);
        var restored = converter.ConvertFromStorage(payload);

        Assert.True(restored.TryGetValue("key", out var value));
        Assert.Equal("value", value?.ToString());
    }
}
