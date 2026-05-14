namespace StateForge.Persistence.EntityFrameworkCore.Serialization;

/// <summary>
/// Converts <typeparamref name="TState" /> values to/from durable string storage.
/// </summary>
public interface IStateValueConverter<TState>
{
    string ConvertToStorage(TState state);

    TState ConvertFromStorage(string value);
}
