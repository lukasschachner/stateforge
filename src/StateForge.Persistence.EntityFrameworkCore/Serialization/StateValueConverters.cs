using System.Text.Json;
using StateForge.Persistence.Snapshots;

namespace StateForge.Persistence.EntityFrameworkCore.Serialization;

public static class StateValueConverters
{
    public static IStateValueConverter<TState> CreateDefault<TState>()
    {
        var type = typeof(TState);
        if (type == typeof(string)) return (IStateValueConverter<TState>)(object)new StringStateValueConverter();
        if (type.IsEnum) return new EnumStateValueConverter<TState>();
        return new JsonStateValueConverter<TState>();
    }

    public static ISnapshotPayloadConverter CreateDefaultPayloadConverter()
    {
        return new JsonSnapshotPayloadConverter();
    }

    private sealed class StringStateValueConverter : IStateValueConverter<string>
    {
        public string ConvertToStorage(string state)
        {
            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("State value is required.", nameof(state));

            return state;
        }

        public string ConvertFromStorage(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("Stored state value is empty.");

            return value;
        }
    }

    private sealed class EnumStateValueConverter<TState> : IStateValueConverter<TState>
    {
        public string ConvertToStorage(TState state)
        {
            return state?.ToString() ?? throw new ArgumentNullException(nameof(state));
        }

        public TState ConvertFromStorage(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("Stored enum state value is empty.");

            if (Enum.TryParse(typeof(TState), value, false, out var parsed) && parsed is TState state)
                return state;

            throw new InvalidOperationException($"Stored state '{value}' is not a valid {typeof(TState).Name} value.");
        }
    }

    private sealed class JsonStateValueConverter<TState> : IStateValueConverter<TState>
    {
        public string ConvertToStorage(TState state)
        {
            return JsonSerializer.Serialize(state);
        }

        public TState ConvertFromStorage(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("Stored JSON state value is empty.");

            var parsed = JsonSerializer.Deserialize<TState>(value);
            return parsed ?? throw new InvalidOperationException("Stored JSON state could not be deserialized.");
        }
    }

    private sealed class JsonSnapshotPayloadConverter : ISnapshotPayloadConverter
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = false
        };

        public string? ConvertToStorage(PersistencePropertyBag properties)
        {
            ArgumentNullException.ThrowIfNull(properties);
            if (properties.Count == 0) return null;

            var map = properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return JsonSerializer.Serialize(map, Options);
        }

        public PersistencePropertyBag ConvertFromStorage(string? payload)
        {
            if (string.IsNullOrWhiteSpace(payload)) return PersistencePropertyBag.Empty;

            var map = JsonSerializer.Deserialize<Dictionary<string, object?>>(payload, Options)
                      ?? throw new InvalidOperationException("Stored payload could not be deserialized.");

            return new PersistencePropertyBag(map.Select(kvp =>
                new KeyValuePair<string, object?>(kvp.Key, kvp.Value)));
        }
    }
}
