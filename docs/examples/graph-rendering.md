# Graph Rendering Example

Optional visualization adapters convert Core graph export data into deterministic text diagrams.

## Supported packages

- `StateForge.Visualization.Mermaid`
- `StateForge.Visualization.Graphviz`
- `StateForge.Visualization.PlantUML`

## Example

```csharp
using StateForge.Visualization.Graphviz.Rendering;
using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.PlantUML.Rendering;

var graph = definition.ExportGraph().Graph!;

var mermaid = MermaidGraphRenderer.Render(graph);
var dot = GraphvizDotRenderer.Render(graph);
var puml = PlantUmlGraphRenderer.Render(graph);

var detailedMermaid = MermaidGraphRenderer.Render(
    graph,
    new MermaidRenderOptions { IncludeMetadata = true });
```

## Output artifacts

The sample in `samples/Graph.RenderingSample` writes:

- `artifacts/graph-rendering/order-flow.mmd`
- `artifacts/graph-rendering/order-flow.dot`
- `artifacts/graph-rendering/order-flow.puml`
- `artifacts/graph-rendering/offer-order-invoice-cancellation-flow.mmd`
- `artifacts/graph-rendering/offer-order-invoice-cancellation-flow.dot`
- `artifacts/graph-rendering/offer-order-invoice-cancellation-flow.puml`

The `offer-order-invoice-cancellation-flow` artifacts demonstrate a larger state-transfer process with:

- Offer handling (`OfferDraft`, `OfferSent`, `OfferRejected`)
- Order handling (`OrderCreated`, `OrderConfirmed`, `OrderCancelled`)
- Invoice handling (`InvoiceDraft`, `InvoiceIssued`, `InvoicePaid`)
- Invoice cancellation subprocess (`InvoiceCancellationRequested`, `InvoiceCancelled`, and rejection back to `InvoiceIssued`)

These are text files only. Rendering images, running browser tools, invoking Graphviz binaries, and hosted rendering services are out of scope.

## Rendering hierarchy data

Renderers consume hierarchy only through Core graph export fields such as `GraphNode.IsComposite`, `ParentChildRelationships`, and `InitialChildMarkers`. Core still contains no renderer-specific layout or styling concepts. Adapters may choose a simple deterministic representation while preserving flat graph output.

## Rendering action summaries

When metadata output is enabled, Mermaid, Graphviz, and PlantUML renderers may include deterministic action-summary annotations. Renderers consume graph summaries only and never execute actions.

## History markers

Renderer packages consume Core graph history metadata. Mermaid, Graphviz, and PlantUML renderers emit deterministic `hierarchy-history` comments containing the composite, history mode, and effective fallback target. Rendering remains metadata-only; Core does not depend on renderer packages.

## Rendering parallel regions and parallel history

Mermaid, Graphviz, and PlantUML adapters consume only renderer-neutral `DefinitionGraph` data for parallel regions. The adapters do not re-run Core validation or inspect runtime internals; region information is emitted from graph metadata and degrades gracefully for non-parallel graphs.

For history-enabled parallel composites, adapters include deterministic metadata comments such as `history=Shallow` or `history=Deep` and the exported fallback state for each region. This is descriptive only: renderers do not infer restore behavior, execute actions, or access runtime history stores. To learn the Core parallel-region modeling semantics before choosing optional visualization adapters, see [parallel regions](parallel-regions.md).

## Rendering runtime overlays

Runtime graph overlays are ignored by default to preserve existing renderer output. Enable adapter-specific overlay hints only when desired:

```csharp
var graph = runtime.ExportGraph().Graph!;

var mermaid = MermaidGraphRenderer.Render(graph, new MermaidRenderOptions
{
    RenderRuntimeOverlay = true
});
```

When enabled, Mermaid, Graphviz, and PlantUML adapters consume only `DefinitionGraph.RuntimeOverlay`. They emit deterministic runtime-overlay comments and active leaf/path hints; they do not inspect runtime instances, dispatch events, evaluate guards, run actions, poll state, or own completion semantics.
