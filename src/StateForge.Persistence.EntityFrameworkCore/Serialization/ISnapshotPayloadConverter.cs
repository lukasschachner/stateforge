using StateForge.Persistence.Snapshots;

namespace StateForge.Persistence.EntityFrameworkCore.Serialization;

/// <summary>
/// Converts persistence property bags to/from optional durable payload text.
/// </summary>
public interface ISnapshotPayloadConverter
{
    string? ConvertToStorage(PersistencePropertyBag properties);

    PersistencePropertyBag ConvertFromStorage(string? payload);
}
