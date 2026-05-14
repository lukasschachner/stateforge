using StateForge.Core.Definitions;

namespace StateForge.Core.Diagnostics;

/// <summary>A structured participant in a transition conflict diagnostic.</summary>
public sealed class TransitionConflictParticipant
{
    public TransitionConflictParticipant(
        TransitionConflictParticipantRole role,
        string? transitionId = null,
        TransitionTriggerKind triggerKind = TransitionTriggerKind.Event,
        object? @event = null,
        string? eventIdentity = null,
        object? sourceState = null,
        object? targetState = null,
        object? compositeState = null,
        string? regionId = null,
        string? regionName = null,
        GuardOutcomeDiagnostic? guardOutcome = null,
        int order = 0)
    {
        Role = role;
        TransitionId = transitionId;
        TriggerKind = triggerKind;
        Event = @event;
        EventIdentity = eventIdentity;
        SourceState = sourceState;
        TargetState = targetState;
        CompositeState = compositeState;
        RegionId = regionId;
        RegionName = regionName;
        GuardOutcome = guardOutcome;
        Order = order;
    }

    public TransitionConflictParticipantRole Role { get; }
    public string? TransitionId { get; }
    public TransitionTriggerKind TriggerKind { get; }
    public object? Event { get; }
    public string? EventIdentity { get; }
    public object? SourceState { get; }
    public object? TargetState { get; }
    public object? CompositeState { get; }
    public string? RegionId { get; }
    public string? RegionName { get; }
    public GuardOutcomeDiagnostic? GuardOutcome { get; }
    public int Order { get; }
}
