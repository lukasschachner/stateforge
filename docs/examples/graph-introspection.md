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

Graph export includes `DefinitionGraph.Regions`, node region annotations, and edge `RegionClassification` values such as `Regional`, `ParentBoundary`, and `InvalidBoundary`. Region metadata also includes renderer-neutral parallel-history fields: `ParallelHistoryMode`, `ParallelHistorySupported`, and `ParallelHistoryFallbackState`.

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

## Active snapshot vocabulary

Active-shape introspection and active-state snapshots use the same vocabulary:

- `SingleLeaf` means a flat/non-parallel runtime shape with one active leaf.
- `Hierarchical` means a non-parallel shape with an ordered root-to-leaf active path.
- `Parallel` means one owning composite with declaration-ordered active region entries.

`definition.Introspect().GetActiveStateSnapshotKind(state)` can be used by tooling to present the same shape terms that `runtime.CaptureActiveStateSnapshot()` emits, without introducing graph-renderer dependencies.

## Completion Edge Metadata

Completion transitions are exported as renderer-neutral graph edges. Consumers should use `GraphEdge.TriggerKind == GraphTriggerKind.Completion` (or `IsCompletionEdge`) instead of parsing the display label. Completion edges include the source completion scope, target state, `SourceIsComposite`, and `SourceIsParallel` metadata so renderers can style ordinary and parallel completion edges consistently.
