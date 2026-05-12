namespace StateMachineLibrary.Persistence.Snapshots;

/// <summary>
///     Immutable provider-neutral snapshot of a machine instance runtime state.
///     This single-active-state contract remains compatible for flat and non-parallel persistence flows; applications
///     that require hierarchical path or parallel region fidelity can store Core active-state snapshots alongside or
///     instead of this storage-oriented contract as an additive migration.
/// </summary>
/// <typeparam name="TState">Machine state type.</typeparam>
public sealed class StateSnapshot<TState>
{
    /// <summary>
    ///     Creates a snapshot from storage-owned identity, state, and version information.
    /// </summary>
    public StateSnapshot(
        string instanceId,
        string definitionId,
        TState activeState,
        PersistenceVersion version,
        PersistencePropertyBag? properties = null)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
            throw new ArgumentException("Instance id is required.", nameof(instanceId));

        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("Definition id is required.", nameof(definitionId));

        if (version.Value is null) throw new ArgumentException("Version is required.", nameof(version));

        InstanceId = instanceId;
        DefinitionId = definitionId;
        ActiveState = activeState;
        Version = version;
        Properties = properties ?? PersistencePropertyBag.Empty;
    }

    /// <summary>Application identity for the machine instance.</summary>
    public string InstanceId { get; }

    /// <summary>Identity of the definition this snapshot belongs to.</summary>
    public string DefinitionId { get; }

    /// <summary>Active state used when reloading machine execution in the legacy single-state persistence flow.</summary>
    public TState ActiveState { get; }

    /// <summary>Opaque storage version marker used for expected-version saves.</summary>
    public PersistenceVersion Version { get; }

    /// <summary>Optional application-owned metadata not interpreted by the library.</summary>
    public PersistencePropertyBag Properties { get; }
}