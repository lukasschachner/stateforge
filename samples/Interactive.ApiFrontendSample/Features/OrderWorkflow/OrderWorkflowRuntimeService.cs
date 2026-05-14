using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;
using StateForge.Core.Execution;
using StateForge.Core.Introspection;
using StateForge.DependencyInjection.Runtime;
using StateForge.Logging;
using StateForge.Logging.Configuration;
using StateForge.Visualization.Mermaid.Rendering;

namespace Interactive.ApiFrontendSample.Features.OrderWorkflow;

internal sealed class OrderWorkflowRuntimeService : IAsyncDisposable
{
    private readonly StateMachineDefinition<OrderDemoState, OrderDemoEvent> _definition;
    private readonly IStateMachineRuntimeFactory<OrderDemoState, OrderDemoEvent> _runtimeFactory;
    private readonly ITransitionObserver<OrderDemoState, OrderDemoEvent> _loggingObserver;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private StateMachineRuntime<OrderDemoState, OrderDemoEvent> _runtime;
    private decimal _requiredPaymentAmount;
    private decimal _capturedPaymentAmount;

    public OrderWorkflowRuntimeService(
        IStateMachineRuntimeFactoryResolver runtimeFactoryResolver,
        ILogger<OrderWorkflowRuntimeService> logger)
    {
        _runtimeFactory = runtimeFactoryResolver.GetFactory<OrderDemoState, OrderDemoEvent>("interactive-order-workflow");
        _definition = _runtimeFactory.Definition;
        _loggingObserver = logger.CreateStateMachineLoggingObserver<OrderDemoState, OrderDemoEvent>(
            new StateMachineLoggingOptions().UseDefaultSafeDiagnostics());
        _runtime = CreateRuntime();
        ResetPaymentProgress();
    }

    public EventCatalogResponse GetEventCatalog()
    {
        return new EventCatalogResponse(OrderWorkflowEvents.Catalog);
    }

    public async ValueTask<RuntimeStateResponse> GetRuntimeStateAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await BuildRuntimeStateResponseAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask<GraphResponse> GetGraphAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ToGraphResponse(EnsureGraph(_runtime.ExportGraph()));
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask<MermaidDiagramResponse> GetMermaidDiagramAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var graph = EnsureGraph(_runtime.ExportGraph());
            var diagram = MermaidGraphRenderer.Render(graph, new MermaidRenderOptions
            {
                RenderRuntimeOverlay = true,
                IncludeMetadata = false
            });
            return new MermaidDiagramResponse(diagram);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask<PreviewResponse> PreviewAsync(OrderDemoEvent @event,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var preview = await _runtime.PreviewAsync(@event, cancellationToken).ConfigureAwait(false);
            return ToPreviewResponse(preview);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask<ApplyResponse> ApplyAsync(OrderDemoEvent @event,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var outcome = await _runtime.ApplyAsync(@event, cancellationToken).ConfigureAwait(false);
            if (outcome.Committed) UpdatePaymentProgressForCommittedEvent(@event);
            return ToApplyResponse(outcome, BuildPaymentProgress());
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask<RuntimeStateResponse> ResetAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await _runtime.DisposeAsync().ConfigureAwait(false);
            _runtime = CreateRuntime();
            ResetPaymentProgress();
            return await BuildRuntimeStateResponseAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _runtime.DisposeAsync().ConfigureAwait(false);
        _gate.Dispose();
    }

    private StateMachineRuntime<OrderDemoState, OrderDemoEvent> CreateRuntime()
    {
        return _runtimeFactory.Create(OrderDemoState.Draft, ConcurrencyMode.Serialized, _loggingObserver);
    }

    private async ValueTask<RuntimeStateResponse> BuildRuntimeStateResponseAsync(CancellationToken cancellationToken)
    {
        var permittedEvents = await _runtime.GetPermittedEventsAsync(cancellationToken).ConfigureAwait(false);
        var graph = EnsureGraph(_runtime.ExportGraph());
        var overlay = graph.RuntimeOverlay;

        return new RuntimeStateResponse(
            _runtime.CurrentState.ToString(),
            overlay?.IsComplete ?? false,
            ToActiveShapeResponse(_runtime.ActiveStateShape),
            permittedEvents.Select(permitted => ToPublicEventName(permitted.DisplayName))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray(),
            BuildPaymentProgress());
    }

    private GraphResponse ToGraphResponse(DefinitionGraph<OrderDemoState, OrderDemoEvent> graph)
    {
        var overlay = graph.RuntimeOverlay;
        var activeNodeIds = new HashSet<string>(StringComparer.Ordinal);
        var activePathNodeIds = new HashSet<string>(StringComparer.Ordinal);

        if (overlay is not null)
        {
            AddNodeId(activeNodeIds, overlay.ActiveLeafNodeId);
            foreach (var nodeId in overlay.ActivePathNodeIds)
            {
                AddNodeId(activeNodeIds, nodeId);
                AddNodeId(activePathNodeIds, nodeId);
            }

            foreach (var region in overlay.Regions)
            {
                AddNodeId(activeNodeIds, region.ActiveLeafNodeId);
                foreach (var nodeId in region.ActivePathNodeIds)
                {
                    AddNodeId(activeNodeIds, nodeId);
                    AddNodeId(activePathNodeIds, nodeId);
                }
            }
        }

        var nodes = graph.Nodes
            .Select(node => new GraphNodeResponse(
                node.Id,
                node.State.ToString()!,
                node.IsTerminal,
                node.IsComposite,
                node.IsParallelComposite,
                activeNodeIds.Contains(node.Id),
                activePathNodeIds.Contains(node.Id)))
            .ToArray();

        var edges = graph.Edges
            .Select(edge => new GraphEdgeResponse(
                edge.Id,
                edge.SourceNodeId,
                edge.TargetNodeId,
                edge.SourceState.ToString()!,
                edge.TargetState.ToString()!,
                ToPublicEventName(edge.Event.DisplayName),
                edge.TriggerKind.ToString(),
                ToPublicEventName(edge.Label)))
            .ToArray();

        var regions = graph.Regions
            .Select(region => new GraphRegionResponse(
                region.RegionId,
                region.RegionName,
                region.CompositeState.ToString()!,
                region.RegionOrder,
                region.InitialState.ToString()!,
                region.MemberStates.Select(state => state.ToString()!).ToArray(),
                region.TerminalStates.Select(state => state.ToString()!).ToArray()))
            .ToArray();

        return new GraphResponse(nodes, edges, regions, ToOverlayResponse(overlay));
    }

    private PreviewResponse ToPreviewResponse(TransitionPreviewResult<OrderDemoState, OrderDemoEvent> preview)
    {
        var selectedTransitions = preview.ParallelTransitions.Count > 0
            ? preview.ParallelTransitions
                .Select(ToTransitionSummary)
                .ToArray()
            : preview.SelectedTransition is null
                ? []
                : [ToTransitionSummary(preview.SelectedTransition)];

        var guardDiagnostics = preview.GuardDiagnostics.Count > 0
            ? preview.GuardDiagnostics.Select(ToGuardDiagnostic).ToArray()
            : preview.DenialDiagnostic?.GuardDiagnostics.Select(ToGuardDiagnostic).ToArray() ?? [];

        return new PreviewResponse(
            preview.Status.ToString(),
            preview.IsPermitted,
            preview.IsPermitted ? preview.ExpectedTargetState.ToString() : null,
            ToActiveShapeResponse(preview.CurrentActiveShape),
            preview.ExpectedActiveShape is null ? null : ToActiveShapeResponse(preview.ExpectedActiveShape),
            preview.PredictionCompleteness.ToString(),
            selectedTransitions,
            preview.CandidateTransitions
                .Select(candidate => candidate.TransitionId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToArray(),
            preview.DenialDiagnostic?.Reason.ToString(),
            preview.DenialDiagnostic?.Message,
            guardDiagnostics);
    }

    private ApplyResponse ToApplyResponse(TransitionOutcome<OrderDemoState, OrderDemoEvent> outcome,
        PaymentProgressResponse paymentProgress)
    {
        return new ApplyResponse(
            outcome.Category.ToString(),
            outcome.Committed,
            outcome.PreviousState.ToString()!,
            outcome.ResultingState.ToString()!,
            outcome.Diagnostics.Summary,
            ToActiveShapeResponse(outcome.ActiveStateShape),
            outcome.ParallelTransitions
                .Select(ToTransitionSummary)
                .ToArray(),
            outcome.DenialDiagnostics
                .Select(ToDenialDiagnostic)
                .ToArray(),
            paymentProgress);
    }

    private ActiveShapeResponse ToActiveShapeResponse(ActiveStateShape<OrderDemoState> shape)
    {
        if (shape.IsParallel)
        {
            var regions = shape.ActiveRegions
                .Select(region => new ActiveRegionResponse(
                    region.RegionId,
                    region.RegionName,
                    region.ActiveLeafState.ToString()!,
                    region.ActivePath.StatesRootToLeaf.Select(state => state.ToString()!).ToArray(),
                    region.IsTerminal,
                    region.IsTerminal))
                .ToArray();

            var activeLeaf = regions.FirstOrDefault()?.ActiveLeafState;
            var owningState = shape.OwningCompositeState.ToString();
            var owningPath = _definition.GetActiveStatePath(shape.OwningCompositeState)
                .StatesRootToLeaf
                .Select(state => state.ToString()!)
                .ToArray();

            return new ActiveShapeResponse(
                shape.Kind.ToString(),
                shape.Sequence,
                activeLeaf,
                owningState,
                owningPath,
                regions);
        }

        var leaf = shape.ActiveLeafState!;
        return new ActiveShapeResponse(
            shape.Kind.ToString(),
            shape.Sequence,
            leaf.ToString(),
            null,
            _definition.GetActiveStatePath(leaf)
                .StatesRootToLeaf
                .Select(state => state.ToString()!)
                .ToArray(),
            []);
    }

    private static GraphOverlayResponse? ToOverlayResponse(GraphActiveStateOverlay<OrderDemoState>? overlay)
    {
        if (overlay is null) return null;

        return new GraphOverlayResponse(
            overlay.ShapeKind.ToString(),
            overlay.Sequence,
            overlay.ActiveLeafNodeId,
            overlay.ActivePathNodeIds.ToArray(),
            overlay.IsComplete,
            overlay.Regions
                .Select(region => new GraphRegionOverlayResponse(
                    region.RegionId,
                    region.RegionName ?? region.RegionId,
                    region.RegionOrder,
                    region.ActiveLeafNodeId,
                    region.IsTerminal,
                    region.IsComplete))
                .ToArray());
    }

    private static TransitionSummaryResponse ToTransitionSummary(
        TransitionDefinition<OrderDemoState, OrderDemoEvent> transition)
    {
        return new TransitionSummaryResponse(
            transition.SourceState.ToString()!,
            transition.TargetState.ToString()!,
            ToPublicEventName(transition.Event.DisplayName),
            transition.TriggerKind.ToString());
    }

    private static string ToPublicEventName(string eventDisplayName)
    {
        return eventDisplayName is nameof(PartialCapturePayment) or nameof(FinalCapturePayment)
            ? nameof(CapturePayment)
            : eventDisplayName;
    }

    private static GuardDiagnosticResponse ToGuardDiagnostic(TransitionPreviewGuardDiagnostic guard)
    {
        return new GuardDiagnosticResponse(
            guard.TransitionId,
            guard.GuardIndex,
            guard.DisplayName,
            guard.Status.ToString(),
            guard.Message);
    }

    private static DenialDiagnosticResponse ToDenialDiagnostic(TransitionDenialDiagnostic denial)
    {
        return new DenialDiagnosticResponse(
            denial.Reason.ToString(),
            denial.Message,
            denial.Phase.ToString(),
            denial.EventIdentity,
            denial.TransitionId,
            denial.GuardDiagnostics.Select(ToGuardDiagnostic).ToArray());
    }

    private void ResetPaymentProgress()
    {
        _requiredPaymentAmount = 0m;
        _capturedPaymentAmount = 0m;
    }

    private void UpdatePaymentProgressForCommittedEvent(OrderDemoEvent @event)
    {
        switch (@event)
        {
            case SubmitOrder submit:
                _requiredPaymentAmount = Math.Max(0m, submit.TotalAmount);
                _capturedPaymentAmount = 0m;
                break;
            case CapturePayment capture:
                _requiredPaymentAmount = capture.RequiredAmount > 0m
                    ? capture.RequiredAmount
                    : _requiredPaymentAmount;
                _capturedPaymentAmount = Math.Max(_capturedPaymentAmount, capture.CapturedTotal);
                break;
        }

        if (_requiredPaymentAmount > 0m && _capturedPaymentAmount > _requiredPaymentAmount)
            _capturedPaymentAmount = _requiredPaymentAmount;
    }

    private PaymentProgressResponse BuildPaymentProgress()
    {
        var required = Math.Max(0m, _requiredPaymentAmount);
        var captured = Math.Max(0m, _capturedPaymentAmount);
        var remaining = required > 0m ? Math.Max(0m, required - captured) : 0m;
        var progress = required > 0m ? Math.Min(100m, Math.Round(captured / required * 100m, 2)) : 0m;
        var isComplete = required > 0m && captured >= required;

        return new PaymentProgressResponse(required, captured, remaining, progress, isComplete);
    }

    private static DefinitionGraph<OrderDemoState, OrderDemoEvent> EnsureGraph(
        GraphExportResult<OrderDemoState, OrderDemoEvent> export)
    {
        if (export.Succeeded && export.Graph is not null) return export.Graph;

        throw new InvalidOperationException(export.FailureSummary ?? "Runtime graph export failed.");
    }

    private static void AddNodeId(HashSet<string> set, string? nodeId)
    {
        if (!string.IsNullOrWhiteSpace(nodeId)) set.Add(nodeId);
    }
}
