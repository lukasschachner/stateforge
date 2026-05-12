using StateMachineLibrary.Core.Execution;

namespace StateMachineLibrary.Persistence.Snapshots;

/// <summary>Candidate snapshot produced by a successful transition before storage commits it.</summary>
public sealed class ProposedSnapshot<TState, TEvent>
{
    public ProposedSnapshot(
        string instanceId,
        string definitionId,
        TState activeState,
        PersistenceVersion version,
        PersistenceVersion previousVersion,
        TransitionOutcome<TState, TEvent> transitionOutcome,
        PersistencePropertyBag? properties = null)
        : this(
            new StateSnapshot<TState>(instanceId, definitionId, activeState, version, properties),
            previousVersion,
            transitionOutcome)
    {
    }

    public ProposedSnapshot(
        StateSnapshot<TState> snapshot,
        PersistenceVersion previousVersion,
        TransitionOutcome<TState, TEvent> transitionOutcome)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));

        if (previousVersion.Value is null)
            throw new ArgumentException("Previous version is required.", nameof(previousVersion));

        PreviousVersion = previousVersion;
        TransitionOutcome = transitionOutcome ?? throw new ArgumentNullException(nameof(transitionOutcome));
    }

    /// <summary>Underlying proposed state snapshot to pass to application storage.</summary>
    public StateSnapshot<TState> Snapshot { get; }

    public string InstanceId => Snapshot.InstanceId;
    public string DefinitionId => Snapshot.DefinitionId;
    public TState ActiveState => Snapshot.ActiveState;
    public PersistenceVersion Version => Snapshot.Version;
    public PersistencePropertyBag Properties => Snapshot.Properties;
    public PersistenceVersion PreviousVersion { get; }
    public TransitionOutcome<TState, TEvent> TransitionOutcome { get; }
}

/// <summary>Authoritative snapshot accepted by storage.</summary>
public sealed class CommittedSnapshot<TState>
{
    public CommittedSnapshot(
        string instanceId,
        string definitionId,
        TState activeState,
        PersistenceVersion committedVersion,
        PersistencePropertyBag? properties = null)
        : this(new StateSnapshot<TState>(instanceId, definitionId, activeState, committedVersion, properties))
    {
    }

    public CommittedSnapshot(StateSnapshot<TState> snapshot)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));

        if (snapshot.Version.Value is null)
            throw new ArgumentException("Committed version is required.", nameof(snapshot));

        CommittedVersion = snapshot.Version;
    }

    /// <summary>Underlying authoritative state snapshot accepted by storage.</summary>
    public StateSnapshot<TState> Snapshot { get; }

    public string InstanceId => Snapshot.InstanceId;
    public string DefinitionId => Snapshot.DefinitionId;
    public TState ActiveState => Snapshot.ActiveState;
    public PersistenceVersion Version => Snapshot.Version;
    public PersistencePropertyBag Properties => Snapshot.Properties;
    public PersistenceVersion CommittedVersion { get; }
}