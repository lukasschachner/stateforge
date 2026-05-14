namespace StateForge.Persistence.Snapshots;

/// <summary>Opaque optimistic concurrency version marker supplied by application storage.</summary>
public readonly record struct PersistenceVersion
{
    public PersistenceVersion(object value, string? displayValue = null)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        DisplayValue = string.IsNullOrWhiteSpace(displayValue) ? value.ToString() : displayValue;
    }

    public object Value { get; }
    public string? DisplayValue { get; }

    public static PersistenceVersion From(object value, string? displayValue = null)
    {
        return new PersistenceVersion(value, displayValue);
    }

    public override string ToString()
    {
        return DisplayValue ?? Value.ToString() ?? "<version>";
    }
}