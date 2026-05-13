namespace StateMachineLibrary.SourceGenerators.Declarations;

public sealed class GeneratedMetadataModel
{
    public GeneratedMetadataModel(string machineId, IReadOnlyList<GeneratedStateMetadata> states,
        IReadOnlyList<GeneratedEventMetadata> events, IReadOnlyList<GeneratedTransitionMetadata> transitions)
    {
        if (string.IsNullOrWhiteSpace(machineId)) throw new ArgumentException("Machine ID is required.", nameof(machineId));
        MachineId = machineId;
        States = states ?? throw new ArgumentNullException(nameof(states));
        Events = events ?? throw new ArgumentNullException(nameof(events));
        Transitions = transitions ?? throw new ArgumentNullException(nameof(transitions));
    }

    public string MachineId { get; }
    public IReadOnlyList<GeneratedStateMetadata> States { get; }
    public IReadOnlyList<GeneratedEventMetadata> Events { get; }
    public IReadOnlyList<GeneratedTransitionMetadata> Transitions { get; }
}

public sealed class GeneratedStateMetadata
{
    public GeneratedStateMetadata(string identityKey, string displayName, bool isTerminal)
    {
        if (string.IsNullOrWhiteSpace(identityKey)) throw new ArgumentException("Identity key is required.", nameof(identityKey));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name is required.", nameof(displayName));
        IdentityKey = identityKey;
        DisplayName = displayName;
        IsTerminal = isTerminal;
    }

    public string IdentityKey { get; }
    public string DisplayName { get; }
    public bool IsTerminal { get; }
}

public sealed class GeneratedEventMetadata
{
    public GeneratedEventMetadata(string identityKey, string displayName, DeclaredEventKind eventKind,
        GeneratedHelperAvailability helperAvailability, GeneratedHelperSkippedReason skippedReason)
    {
        if (string.IsNullOrWhiteSpace(identityKey)) throw new ArgumentException("Identity key is required.", nameof(identityKey));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name is required.", nameof(displayName));
        IdentityKey = identityKey;
        DisplayName = displayName;
        EventKind = eventKind;
        HelperAvailability = helperAvailability;
        SkippedReason = skippedReason;
    }

    public string IdentityKey { get; }
    public string DisplayName { get; }
    public DeclaredEventKind EventKind { get; }
    public GeneratedHelperAvailability HelperAvailability { get; }
    public GeneratedHelperSkippedReason SkippedReason { get; }
}

public sealed class GeneratedTransitionMetadata
{
    public GeneratedTransitionMetadata(string transitionId, string sourceStateKey, string eventKey, string targetStateKey,
        DeclaredTransitionKind transitionKind)
    {
        if (string.IsNullOrWhiteSpace(transitionId)) throw new ArgumentException("Transition ID is required.", nameof(transitionId));
        if (string.IsNullOrWhiteSpace(sourceStateKey)) throw new ArgumentException("Source state key is required.", nameof(sourceStateKey));
        if (string.IsNullOrWhiteSpace(eventKey)) throw new ArgumentException("Event key is required.", nameof(eventKey));
        if (string.IsNullOrWhiteSpace(targetStateKey)) throw new ArgumentException("Target state key is required.", nameof(targetStateKey));
        TransitionId = transitionId;
        SourceStateKey = sourceStateKey;
        EventKey = eventKey;
        TargetStateKey = targetStateKey;
        TransitionKind = transitionKind;
    }

    public string TransitionId { get; }
    public string SourceStateKey { get; }
    public string EventKey { get; }
    public string TargetStateKey { get; }
    public DeclaredTransitionKind TransitionKind { get; }
}
