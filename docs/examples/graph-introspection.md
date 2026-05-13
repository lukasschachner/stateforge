# Graph Introspection Example

Core graph export returns structured definition data that applications can inspect or adapt. It does not render diagrams, calculate layout, or depend on visualization packages.

Run the validated sample:

```bash
dotnet run --project samples/Graph.IntrospectionSample/Graph.IntrospectionSample.csproj --configuration Release
```

The sample creates a typed finite state machine, calls `ExportGraph()`, iterates `graph.Nodes` and `graph.Edges`, and prints transition relationships. Consumers can transform this data into their own documentation or visualization layer outside the Core package boundary.

For optional deterministic text diagram adapters, see [`docs/examples/graph-rendering.md`](./graph-rendering.md).

## Hierarchy metadata

For hierarchical definitions, graph export includes dependency-light hierarchy data without executing the machine:

- `graph.Hierarchy` indicates whether hierarchy is present and how many relationships/initial markers were exported.
- `graph.ParentChildRelationships` lists composite-to-child containment records.
- `graph.InitialChildMarkers` records each composite state's initial child and resolved initial leaf path.
- `GraphNode` exposes `IsComposite`, `IsLeaf`, parent, child count, and initial-child fields.
- `GraphEdge` exposes composite source/target and resolved-target metadata.

Flat definitions receive safe defaults and existing node/edge consumers remain compatible.

## Action summaries

Graph export includes non-executable action summaries on nodes (`EntryActions`, `ExitActions`) and edges (`TransitionActions`). Exporting a graph never invokes action delegates.

## History metadata

History-enabled composites are exposed through definition introspection and graph export. `definition.Introspect().HistoryEnabledStates` lists composites with `HistoryMode`, fallback metadata, and hierarchy information. Exported graphs include `HistoryMarkers` and `GraphNode.HasHistory` fields so tooling can discover shallow/deep history without executing actions or guards.

## Parallel-region and parallel-history metadata

Graph export includes `DefinitionGraph.Regions`, node region annotations, and edge `RegionClassification` values such as `Regional`, `ParentBoundary`, and `InvalidBoundary`. Region metadata also includes renderer-neutral parallel-history fields: `ParallelHistoryMode`, `ParallelHistorySupported`, and `ParallelHistoryFallbackState`. For the user-facing modeling guide and runnable order-processing sample, see [parallel regions](parallel-regions.md).

Definition introspection exposes parallel-history configuration separately:

```csharp
foreach (var history in definition.Introspect().ParallelHistoryDefinitions)
{
    Console.WriteLine($"{history.CompositeState}: {history.HistoryMode}");
    foreach (var fallback in history.RegionFallbacks)
    {
        Console.WriteLine($"  {fallback.RegionName} fallback={fallback.FallbackState}");
    }
}
```

Runtime code can inspect `StateMachineRuntime.ActiveStateShape` to enumerate current active region entries and `StateMachineRuntime.ParallelHistorySnapshots` to enumerate recorded history entries. These views are intentionally distinct: active shape is current execution state, while snapshots are provider-neutral recorded history.

## Runtime graph overlays

Runtime instances can export the same renderer-neutral definition graph data with an additive active-state overlay:

```csharp
var runtime = definition.CreateRuntime(OrderState.Created);
var export = runtime.ExportGraph();
var overlay = export.Graph!.RuntimeOverlay!;

Console.WriteLine($"active={overlay.ActiveLeafState} sequence={overlay.Sequence}");
```

`RuntimeGraphExportOptions` controls whether the overlay is included. `OverlayMode = RuntimeGraphOverlayMode.None` returns definition graph data with `RuntimeOverlay == null`, preserving static graph behavior through runtime export plumbing.

The overlay is inspection-only: export does not dispatch events, evaluate guards, run actions, invoke observers, or mutate history. Accessor-backed runtimes expose `ExportGraphAsync(...)`, read the current state through the accessor, honor cancellation, and do not call the accessor write path.

For hierarchical machines, the overlay includes `ActivePath` and matching graph node ids. For parallel machines, it includes declaration-ordered region overlays with active leaves, active paths, terminal/completion status, completed region ids, and the captured sequence.

## Active snapshot vocabulary

Active-shape introspection and active-state snapshots use the same vocabulary:

- `SingleLeaf` means a flat/non-parallel runtime shape with one active leaf.
- `Hierarchical` means a non-parallel shape with an ordered root-to-leaf active path.
- `Parallel` means one owning composite with declaration-ordered active region entries.

`definition.Introspect().GetActiveStateSnapshotKind(state)` can be used by tooling to present the same shape terms that `runtime.CaptureActiveStateSnapshot()` emits, without introducing graph-renderer dependencies.

## Completion Edge Metadata

Completion transitions are exported as renderer-neutral graph edges. Consumers should use `GraphEdge.TriggerKind == GraphTriggerKind.Completion` (or `IsCompletionEdge`) instead of parsing the display label. Completion edges include the source completion scope, target state, `SourceIsComposite`, and `SourceIsParallel` metadata so renderers can style ordinary and parallel completion edges consistently.
