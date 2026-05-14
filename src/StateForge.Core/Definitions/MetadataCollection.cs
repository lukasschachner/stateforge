using System.Collections;

namespace StateForge.Core.Definitions;

/// <summary>Immutable metadata container for documentation, visualization, and extensions.</summary>
public sealed class MetadataCollection : IReadOnlyDictionary<string, object?>
{
    private readonly IReadOnlyDictionary<string, object?> _items;

    public MetadataCollection() : this(new Dictionary<string, object?>())
    {
    }

    public MetadataCollection(IEnumerable<KeyValuePair<string, object?>> items)
    {
        _items = new Dictionary<string, object?>(items, StringComparer.Ordinal);
    }

    public static MetadataCollection Empty { get; } = new();

    public object? this[string key] => _items[key];
    public IEnumerable<string> Keys => _items.Keys;
    public IEnumerable<object?> Values => _items.Values;
    public int Count => _items.Count;

    public bool ContainsKey(string key)
    {
        return _items.ContainsKey(key);
    }

    public bool TryGetValue(string key, out object? value)
    {
        return _items.TryGetValue(key, out value);
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public MetadataCollection With(string key, object? value)
    {
        var copy = new Dictionary<string, object?>(_items, StringComparer.Ordinal) { [key] = value };
        return new MetadataCollection(copy);
    }
}