using StateForge.Core.Definitions;
using StateForge.Persistence.Diagnostics;
using StateForge.Persistence.Snapshots;
using StateForge.Persistence.Storage;

namespace StateForge.Persistence.Execution;

/// <summary>
///     Coordinates snapshot reload against an application-owned snapshot store and machine definition.
/// </summary>
public static class PersistentStateLoader
{
    /// <summary>
    ///     Reloads a stored single-state snapshot and validates definition identity and active-state compatibility.
    ///     Active-shape snapshots for hierarchical or parallel fidelity can be validated separately through Core
    ///     before creating a runtime, while this loader preserves existing single-state persistence behavior.
    /// </summary>
    public static async ValueTask<SnapshotLoadResult<TState>> ReloadAsync<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        string instanceId,
        IStateSnapshotStore<TState> store,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(store);

        var definitionId = definition.GetPersistenceDefinitionId();
        var load = await store.LoadAsync(instanceId, definitionId, cancellationToken).ConfigureAwait(false);
        if (load.Category != SnapshotLoadCategory.Loaded) return load;

        var snapshot = load.Snapshot!;
        var validation = StateSnapshotValidator.Validate(definition, snapshot, definitionId);
        if (!validation.IsValid)
            return SnapshotLoadResult<TState>.InvalidSnapshot(new PersistenceDiagnostics(
                "Stored snapshot is incompatible with the supplied machine definition.",
                "snapshot.invalid",
                validationIssues: validation.Issues));

        return load;
    }
}