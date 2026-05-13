using System.Text.Json;

namespace Interactive.ApiFrontendSample.Features.OrderWorkflow;

internal sealed record DemoEventRequest(string EventType, JsonElement Payload);

internal sealed record EventCatalogResponse(IReadOnlyList<EventDescriptorResponse> Events);

internal sealed record EventDescriptorResponse(
    string EventType,
    string Description,
    IReadOnlyList<EventFieldResponse> Fields,
    IReadOnlyDictionary<string, object?> ExamplePayload);

internal sealed record EventFieldResponse(string Name, string Type, string Hint, bool Required);

internal sealed record RuntimeStateResponse(
    string CurrentState,
    bool IsComplete,
    ActiveShapeResponse ActiveShape,
    IReadOnlyList<string> PermittedEvents,
    PaymentProgressResponse PaymentProgress);

internal sealed record PaymentProgressResponse(
    decimal RequiredAmount,
    decimal CapturedAmount,
    decimal RemainingAmount,
    decimal ProgressPercent,
    bool IsPaymentComplete);

internal sealed record ActiveShapeResponse(
    string Kind,
    long Sequence,
    string? ActiveLeafState,
    string? OwningCompositeState,
    IReadOnlyList<string> ActivePath,
    IReadOnlyList<ActiveRegionResponse> Regions);

internal sealed record ActiveRegionResponse(
    string RegionId,
    string RegionName,
    string ActiveLeafState,
    IReadOnlyList<string> ActivePath,
    bool IsTerminal,
    bool IsComplete);

internal sealed record GraphResponse(
    IReadOnlyList<GraphNodeResponse> Nodes,
    IReadOnlyList<GraphEdgeResponse> Edges,
    IReadOnlyList<GraphRegionResponse> Regions,
    GraphOverlayResponse? Overlay);

internal sealed record MermaidDiagramResponse(string Diagram);

internal sealed record GraphNodeResponse(
    string Id,
    string State,
    bool IsTerminal,
    bool IsComposite,
    bool IsParallelComposite,
    bool IsActive,
    bool IsInActivePath);

internal sealed record GraphEdgeResponse(
    string Id,
    string SourceNodeId,
    string TargetNodeId,
    string SourceState,
    string TargetState,
    string EventDisplayName,
    string TriggerKind,
    string Label);

internal sealed record GraphRegionResponse(
    string RegionId,
    string RegionName,
    string OwnerState,
    int Order,
    string InitialState,
    IReadOnlyList<string> MemberStates,
    IReadOnlyList<string> TerminalStates);

internal sealed record GraphOverlayResponse(
    string ShapeKind,
    long Sequence,
    string? ActiveLeafNodeId,
    IReadOnlyList<string> ActivePathNodeIds,
    bool IsComplete,
    IReadOnlyList<GraphRegionOverlayResponse> Regions);

internal sealed record GraphRegionOverlayResponse(
    string RegionId,
    string RegionName,
    int RegionOrder,
    string? ActiveLeafNodeId,
    bool IsTerminal,
    bool IsComplete);

internal sealed record TransitionSummaryResponse(
    string SourceState,
    string TargetState,
    string EventDisplayName,
    string TriggerKind);

internal sealed record GuardDiagnosticResponse(
    string? TransitionId,
    int GuardIndex,
    string DisplayName,
    string Status,
    string? Message);

internal sealed record DenialDiagnosticResponse(
    string Reason,
    string Message,
    string Phase,
    string? EventIdentity,
    string? TransitionId,
    IReadOnlyList<GuardDiagnosticResponse> GuardDiagnostics);

internal sealed record PreviewResponse(
    string Status,
    bool IsPermitted,
    string? ExpectedTargetState,
    ActiveShapeResponse CurrentActiveShape,
    ActiveShapeResponse? ExpectedActiveShape,
    string PredictionCompleteness,
    IReadOnlyList<TransitionSummaryResponse> SelectedTransitions,
    IReadOnlyList<string> CandidateTransitionIds,
    string? DenialReason,
    string? DenialMessage,
    IReadOnlyList<GuardDiagnosticResponse> GuardDiagnostics);

internal sealed record ApplyResponse(
    string Category,
    bool Committed,
    string PreviousState,
    string ResultingState,
    string Summary,
    ActiveShapeResponse ActiveShape,
    IReadOnlyList<TransitionSummaryResponse> Transitions,
    IReadOnlyList<DenialDiagnosticResponse> DenialDiagnostics,
    PaymentProgressResponse PaymentProgress);
