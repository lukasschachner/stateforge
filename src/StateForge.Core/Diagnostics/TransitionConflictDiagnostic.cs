using StateForge.Core.Definitions;

namespace StateForge.Core.Diagnostics;

/// <summary>Structured machine-readable details for a validation-time or runtime transition conflict.</summary>
public sealed class TransitionConflictDiagnostic
{
    public TransitionConflictDiagnostic(
        TransitionConflictKind kind,
        string message,
        TransitionTriggerKind triggerKind = TransitionTriggerKind.Event,
        object? @event = null,
        string? eventIdentity = null,
        object? completionScope = null,
        object? conflictScope = null,
        object? compositeState = null,
        string? sourceRegionId = null,
        string? sourceRegionName = null,
        string? targetRegionId = null,
        string? targetRegionName = null,
        IEnumerable<TransitionConflictParticipant>? participants = null,
        InvalidActiveShapeDiagnostic? invalidShape = null,
        string? validationCode = null)
    {
        Kind = kind;
        Message = message;
        TriggerKind = triggerKind;
        Event = @event;
        EventIdentity = eventIdentity;
        CompletionScope = completionScope;
        ConflictScope = conflictScope;
        CompositeState = compositeState;
        SourceRegionId = sourceRegionId;
        SourceRegionName = sourceRegionName;
        TargetRegionId = targetRegionId;
        TargetRegionName = targetRegionName;
        Participants = (participants ?? []).OrderBy(p => p.Order).ToArray();
        InvalidShape = invalidShape;
        ValidationCode = validationCode;
    }

    public TransitionConflictKind Kind { get; }
    public string Message { get; }
    public TransitionTriggerKind TriggerKind { get; }
    public object? Event { get; }
    public string? EventIdentity { get; }
    public object? CompletionScope { get; }
    public object? ConflictScope { get; }
    public object? CompositeState { get; }
    public string? SourceRegionId { get; }
    public string? SourceRegionName { get; }
    public string? TargetRegionId { get; }
    public string? TargetRegionName { get; }
    public IReadOnlyList<TransitionConflictParticipant> Participants { get; }
    public InvalidActiveShapeDiagnostic? InvalidShape { get; }
    public string? ValidationCode { get; }
}
