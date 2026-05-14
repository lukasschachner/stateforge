using StateForge.Core.Definitions;

namespace StateForge.Persistence;

/// <summary>Helpers for reading persistence-related metadata from machine definitions.</summary>
public static class PersistenceDefinitionExtensions
{
    /// <summary>
    ///     Returns the persistence definition identity declared in definition metadata, or a stable type fallback.
    /// </summary>
    public static string GetPersistenceDefinitionId<TState, TEvent>(
        this StateMachineDefinition<TState, TEvent> definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (definition.Metadata.TryGetValue(PersistenceMetadataKeys.DefinitionId, out var value) &&
            !string.IsNullOrWhiteSpace(value?.ToString()))
            return value!.ToString()!;

        return definition.GetType().FullName ?? definition.GetType().Name;
    }
}