using System.ComponentModel.DataAnnotations;

namespace StateForge.Persistence.EntityFrameworkCore.Storage;

/// <summary>
/// Durable EF Core row model for one machine instance snapshot.
/// </summary>
public sealed class StateForgeSnapshotRecord
{
    [Key]
    [MaxLength(256)]
    public string InstanceId { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string DefinitionId { get; set; } = string.Empty;

    [Required]
    [MaxLength(1024)]
    public string ActiveState { get; set; } = string.Empty;

    public string? Payload { get; set; }

    /// <summary>
    /// Monotonic optimistic concurrency marker managed by the adapter.
    /// </summary>
    [ConcurrencyCheck]
    public long Version { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
