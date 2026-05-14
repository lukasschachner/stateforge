using System.Collections;

namespace StateForge.Persistence.Snapshots;

/// <summary>Immutable optional metadata bag associated with a state snapshot.</summary>
public sealed class PersistencePropertyBag : IReadOnlyDictionary<string, object?>
{
    private readonly IReadOnlyDictionary<string, object?> _items;

    public PersistencePropertyBag() : this(new Dictionary<string, object?>())
    {
    }

    public PersistencePropertyBag(IEnumerable<KeyValuePair<string, object?>> items)
    {
        _items = new Dictionary<string, object?>(items, StringComparer.Ordinal);
    }

    public static PersistencePropertyBag Empty { get; } = new();

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

    public PersistencePropertyBag With(string key, object? value)
    {
        var copy = new Dictionary<string, object?>(_items, StringComparer.Ordinal)
        {
            [key] = value
        };

        return new PersistencePropertyBag(copy);
    }
}