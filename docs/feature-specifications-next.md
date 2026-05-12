# Future Feature Specifications

This document captures proposed next features for the .NET state machine library after the orthogonal parallel-region implementation. Each section is written as an implementation-ready feature specification with goals, scope, proposed public surface, validation, tests, documentation, and explicit exclusions.

## 1. Parallel-Region History Restore

### Summary

Add first-class history support for parallel composites by storing and restoring the full active region shape instead of a single child or leaf. This closes the current limitation where direct history on a parallel composite is rejected.

### Problem

Existing history semantics restore a single active path. A parallel composite owns multiple active regional paths, so restoring only one child would lose information and could violate the invariant that each region has exactly one active leaf.

### Goals

- Allow direct shallow and deep history on a parallel composite.
- Restore one active leaf per region when re-entering through history.
- Preserve existing non-parallel history behavior unchanged.
- Support nested history inside regions and parallel-composite history at the same time.
- Expose history snapshots through runtime and persistence-friendly APIs.
- Include graph/introspection metadata that identifies history-enabled parallel composites.

### Non-Goals

- No event sourcing provider.
- No database-specific persistence implementation.
- No concurrent regional action scheduler.
- No image rendering changes in Core.

### Proposed Public API

```csharp
builder.ParallelComposite(OrderState.Operational)
    .WithHistory(HistoryMode.Deep)
    .Region("Fulfillment", OrderState.WaitingForPick, [OrderState.Packing])
    .Region("Billing", OrderState.WaitingForPayment, [OrderState.CapturingPayment]);
```

Possible additive runtime types:

```csharp
public sealed record ParallelHistorySnapshot<TState>(
    TState CompositeState,
    IReadOnlyList<ParallelRegionHistoryEntry<TState>> Regions,
    HistoryMode Mode,
    long LastUpdatedSequence);

public sealed record ParallelRegionHistoryEntry<TState>(
    string RegionId,
    string RegionName,
    TState LastActiveLeafState,
    ActiveStatePath<TState> LastActivePath);
```

### Behavioral Requirements

- Entering a parallel composite through history restores all owned regions.
- If a region has no recorded history, it falls back to that region's initial state.
- Shallow history restores each region's direct active child path according to existing shallow semantics.
- Deep history restores each region's deepest recorded active leaf.
- Restored entries must be ordered by region declaration order.
- If history data references removed or invalid states, validation/runtime diagnostics must report the invalid region and state.
- A failed or canceled history restore must not expose a partially restored active shape.

### Validation Requirements

- Direct history on a parallel composite becomes valid only when the definition has valid regions.
- A parallel composite with history still requires all normal parallel-region validation.
- Invalid history fallback configuration must identify the composite, region, fallback state, and mode where applicable.

### Introspection and Graph Requirements

- Definition introspection exposes whether a parallel composite has shallow/deep history.
- Runtime introspection exposes recorded region history entries.
- Graph export includes parallel-history markers as renderer-neutral metadata.
- Visualization adapters may render this metadata as comments or visual hints but must not infer behavior.

### Test Plan

- Restore shallow parallel history after leaving/re-entering a composite.
- Restore deep parallel history with nested composites inside regions.
- Fall back to region initial state when only some regions have recorded history.
- Reject invalid or stale history references.
- Verify cancellation/failure does not partially restore active regions.
- Verify non-parallel history tests remain unchanged.
- Verify public API snapshot updates are intentional.

### Acceptance Criteria

- A direct history-enabled parallel composite restores one active leaf per region.
- Existing history-enabled non-parallel machines pass unchanged.
- Release validation includes docs, samples, and public API snapshot coverage.

---

## 2. First-Class Active-State Persistence Shape

### Summary

Introduce a serializable active-state snapshot abstraction that can represent single-leaf, hierarchical, and parallel-region active configurations.

### Problem

Parallel machines cannot be faithfully persisted as a single `TState` value. Persistence needs a durable representation that captures all active regional leaves, paths, sequence/version metadata, and enough definition identity to validate restores.

### Goals

- Provide a Core-level active-state snapshot that is persistence-provider neutral.
- Represent flat, hierarchical, and parallel active shapes.
- Support round-trip restore into runtime and external runtime contexts.
- Include version/sequence for optimistic concurrency and diagnostics.
- Preserve existing persistence package boundaries.

### Non-Goals

- No database provider in Core.
- No event sourcing stream model.
- No long-running workflow checkpoint scheduler.

### Proposed Public API

```csharp
ActiveStateSnapshot<OrderState> snapshot = runtime.CaptureSnapshot();

using var restored = definition.CreateRuntime(snapshot);
```

Possible types:

```csharp
public sealed class ActiveStateSnapshot<TState>
{
    public ActiveStateSnapshotKind Kind { get; }
    public TState? SingleActiveLeaf { get; }
    public IReadOnlyList<TState> ActivePath { get; }
    public TState? OwningCompositeState { get; }
    public IReadOnlyList<ActiveRegionSnapshot<TState>> Regions { get; }
    public long Sequence { get; }
    public string? DefinitionFingerprint { get; }
}

public sealed record ActiveRegionSnapshot<TState>(
    string RegionId,
    string RegionName,
    TState ActiveLeafState,
    IReadOnlyList<TState> ActivePath,
    bool IsTerminal);
```

### Behavioral Requirements

- Capturing a snapshot is side-effect free.
- Restoring validates that referenced states and region IDs exist.
- Parallel snapshots restore one active region entry per region.
- Missing, duplicate, or unknown region entries fail validation before runtime use.
- Flat snapshots remain simple and do not require parallel metadata.

### Persistence Package Requirements

- Persistence package stores the new active-state snapshot without depending on visualization packages.
- Existing single-state persistence APIs remain available for non-parallel machines.
- Migration guidance explains how to move from single-state persistence to active-shape persistence.

### Test Plan

- Capture/restore flat machine snapshot.
- Capture/restore hierarchical active path.
- Capture/restore parallel active shape with multiple regions.
- Reject snapshot with unknown state.
- Reject snapshot with missing region entry.
- Validate persistence package boundary remains unchanged.

### Acceptance Criteria

- Parallel active-state shape can be persisted and restored without losing regional active leaves.
- Existing single-state persistence tests remain compatible.
- Snapshot types are documented and included in API snapshots.

---

## 3. Completion Events and Explicit Completion Transitions

### Summary

Add explicit completion-transition modeling, such as `.OnCompletion().GoTo(...)`, for ordinary composites and parallel composites.

### Problem

Completion behavior is currently implicit or modeled through ordinary events. Users need a clear, type-safe way to say: when this composite completes, transition to another state.

### Goals

- Add explicit completion transition builder APIs.
- Support completion transitions for ordinary hierarchical composites and parallel composites.
- Preserve existing event-triggered transitions.
- Make completion transition ordering, guard evaluation, and action lifecycle deterministic.
- Expose completion transitions in introspection and graph export.

### Non-Goals

- No workflow joins beyond FSM completion semantics.
- No timed or scheduled completion triggers.

### Proposed Public API

```csharp
builder.State(OrderState.Reviewing)
    .InitialChild(OrderState.AuthorReview)
    .OnCompletion().GoTo(OrderState.Approved);

builder.ParallelComposite(OrderState.Operational)
    .Region("Fulfillment", OrderState.WaitingForPick, terminalStates: [OrderState.FulfillmentDone])
    .Region("Billing", OrderState.WaitingForPayment, terminalStates: [OrderState.BillingDone])
    .OnCompletion().GoTo(OrderState.ReadyToClose);
```

Possible event descriptor:

```csharp
public sealed class CompletionEventDefinition<TEvent> : EventDefinition<TEvent>
```

or internal sentinel event represented separately from `TEvent`.

### Behavioral Requirements

- Completion transitions run after terminal entry actions complete.
- Parallel composite completion occurs only after every region is terminal.
- Completion is recognized exactly once per completion episode.
- Completion guards/actions follow existing lifecycle semantics.
- Completion transition conflicts are detected before commit.

### Validation Requirements

- At most one unguarded completion transition per completion scope unless deterministic priority rules apply.
- Completion transition source must be a composite or terminal scope where completion is meaningful.
- Completion transition target must be a valid state.

### Introspection and Graph Requirements

- Graph edges classify completion transitions as `Completion`.
- Definition introspection lists outgoing completion transitions separately from event transitions.
- Documentation explains completion ordering relative to entry actions.

### Test Plan

- Ordinary composite completion transition.
- Parallel all-regions-terminal completion transition.
- Guarded completion transition.
- Ambiguous completion transition validation.
- Completion transition action ordering.
- Cancellation/failure before and after commit.

### Acceptance Criteria

- Users can model completion without inventing artificial events.
- Graph export clearly distinguishes completion edges.
- Existing event transition behavior remains unchanged.

---

## 4. Source Generator Support for Hierarchy and Parallel Regions

### Summary

Extend source generators so compile-time declarations can express hierarchy, history, terminal states, parallel composites, regions, and region membership.

### Problem

The fluent builder can model advanced definitions, but source-generated definitions should offer the same expressive power for users who prefer attributes or DSL declarations.

### Goals

- Add source-generator syntax for hierarchical parent/child relationships.
- Add syntax for initial child and history settings.
- Add syntax for parallel composites and named regions.
- Add syntax for regional terminal states.
- Emit equivalent builder calls or immutable definitions.
- Produce compile-time diagnostics for invalid declarations where possible.

### Non-Goals

- No runtime reflection dependency in Core.
- No separate visual designer.

### Attribute API Sketch

```csharp
[StateMachine]
public enum OrderState
{
    Draft,

    [Composite(InitialChild = nameof(WaitingForPick))]
    [ParallelComposite]
    Operational,

    [Region(nameof(Operational), "Fulfillment", IsInitial = true)]
    WaitingForPick,

    [Region(nameof(Operational), "Fulfillment", IsTerminal = true)]
    FulfillmentDone,

    [Region(nameof(Operational), "Billing", IsInitial = true)]
    WaitingForPayment,

    [Region(nameof(Operational), "Billing", IsTerminal = true)]
    BillingDone
}
```

### DSL Sketch

```text
parallel Operational {
  region Fulfillment initial WaitingForPick {
    WaitingForPick -> FulfillmentDone on PickComplete
    terminal FulfillmentDone
  }
  region Billing initial WaitingForPayment {
    WaitingForPayment -> BillingDone on PaymentComplete
    terminal BillingDone
  }
}
```

### Validation and Diagnostics

- Compile-time diagnostics for duplicate region names in the same declaration block.
- Compile-time diagnostics for missing initial state when statically knowable.
- Compile-time diagnostics for region member assigned to multiple regions.
- Runtime validation remains authoritative for guard-dependent ambiguity.

### Test Plan

- Attribute-based parallel composite generation.
- DSL-based parallel composite generation.
- Generated definition validates and runs.
- Compile-time diagnostics for invalid declarations.
- Snapshot tests for generated source output.

### Acceptance Criteria

- Source-generated definitions can express the same parallel-region model as fluent builders.
- Invalid declarations produce actionable diagnostics.
- Existing source generator tests remain compatible.

---

## 5. Richer Transition Conflict Diagnostics

### Summary

Enhance diagnostics for invalid or conflicting transition selection, especially with parallel regions.

### Problem

Parallel dispatch introduces conflicts that are difficult to debug without structured metadata. String summaries are not enough for tooling, logging, or UI integrations.

### Goals

- Add stable conflict categories.
- Include structured participants for each conflict.
- Preserve deterministic ordering of conflict diagnostics.
- Surface region, composite, transition, event, source, and target identities.
- Make diagnostics available on validation results and runtime transition outcomes.

### Non-Goals

- No logging dependency in Core.
- No UI-specific diagnostic formatting.

### Proposed Public API

```csharp
public enum TransitionConflictKind
{
    DuplicateSourceScope,
    ParentRegionalConflict,
    CrossRegionBoundary,
    InvalidPostShape,
    AmbiguousGuardOutcome,
    CompletionConflict
}

public sealed record TransitionConflictDiagnostic<TState, TEvent>(
    TransitionConflictKind Kind,
    TEvent? Event,
    TState? CompositeState,
    IReadOnlyList<string> RegionIds,
    IReadOnlyList<TransitionDefinition<TState, TEvent>> Transitions,
    string Message);
```

`TransitionDiagnostics` could expose:

```csharp
public IReadOnlyList<object> ConflictDiagnostics { get; }
```

or a generic typed diagnostics API if feasible.

### Behavioral Requirements

- Conflict diagnostics must be stable for the same definition, active state, event, and guard outcomes.
- No conflict diagnostic should require renderer-specific data.
- Validation-time and runtime-time diagnostics should use the same code/category vocabulary where possible.

### Test Plan

- Parent-vs-regional conflict includes both parent and regional transitions.
- Cross-region boundary conflict includes source and target region IDs.
- Duplicate source conflict includes all competing transitions.
- Invalid post-shape conflict includes affected region.
- Diagnostics are ordered deterministically.

### Acceptance Criteria

- Conflict outcomes are actionable without parsing strings.
- Existing `TransitionDiagnostics.Summary` remains compatible.
- Public API snapshot documents intentional additions.

---

## 6. Runtime Graph Export with Active-State Overlays

### Summary

Add runtime-aware graph export that overlays active states, active paths, terminal regions, and completion status on top of definition graph data.

### Problem

Definition graph export is useful for static diagrams, but debugging a running machine requires knowing the active leaf or active regional leaves.

### Goals

- Export graph data from a runtime instance.
- Mark active single leaf for flat/hierarchical machines.
- Mark active leaves per parallel region.
- Mark completed/terminal regions.
- Keep output renderer-neutral.
- Let visualization adapters consume overlay metadata without runtime internals.

### Non-Goals

- No image rendering in Core.
- No live streaming graph updates.
- No debugger UI.

### Proposed Public API

```csharp
GraphExportResult<TState, TEvent> export = runtime.ExportGraph();
```

Possible metadata types:

```csharp
public sealed record GraphActiveStateOverlay<TState>(
    ActiveStateShapeKind Kind,
    TState? ActiveLeafState,
    IReadOnlyList<GraphActiveRegionOverlay<TState>> ActiveRegions,
    long Sequence);

public sealed record GraphActiveRegionOverlay<TState>(
    string RegionId,
    string RegionName,
    TState ActiveLeafState,
    bool IsTerminal);
```

### Behavioral Requirements

- Export is side-effect free.
- Overlay uses the runtime's current active-state shape snapshot.
- Non-parallel graph export remains unchanged unless runtime overlay is explicitly requested.
- Overlay data must be additive.

### Adapter Requirements

- Mermaid/Graphviz/PlantUML adapters may render active states via comments, styling, or optional configuration.
- Adapters must not inspect runtime internals.
- Adapters should gracefully ignore overlays if unsupported.

### Test Plan

- Runtime graph export marks active leaf for flat machine.
- Runtime graph export marks active path for hierarchy.
- Runtime graph export marks active leaves per parallel region.
- Overlay updates after dispatch.
- Static definition graph export remains unchanged.

### Acceptance Criteria

- Users can export graph data that reflects current runtime state.
- Optional adapters can display active-region overlays using only graph data.

---

## 7. Region Builder Ergonomics

### Summary

Improve the fluent builder surface for defining regions, region states, terminal states, and region-scoped transitions with fewer accidental invalid models.

### Problem

Current region APIs are functional but can require users to coordinate membership manually. More expressive region-scoped builders would prevent mistakes and improve discoverability.

### Goals

- Add region-scoped state declarations.
- Add terminal-state helpers inside a region.
- Add strongly named region handles where useful.
- Reduce invalid membership definitions.
- Preserve existing builder APIs.

### Non-Goals

- No breaking changes to current builder methods.
- No visual designer.

### Proposed Public API

```csharp
builder.ParallelComposite(OrderState.Operational, composite =>
{
    composite.Region("Fulfillment", region =>
    {
        region.Initial(OrderState.WaitingForPick);
        region.State(OrderState.WaitingForPick)
            .On(OrderEvent.PickStarted).GoTo(OrderState.Packing);
        region.Terminal(OrderState.FulfillmentDone);
    });

    composite.Region("Billing", region =>
    {
        region.Initial(OrderState.WaitingForPayment);
        region.Terminal(OrderState.BillingDone);
    });
});
```

Possible types:

```csharp
public sealed class ParallelRegionDefinitionBuilder<TState, TEvent>
{
    public ParallelRegionDefinitionBuilder<TState, TEvent> Initial(TState state);
    public StateDefinitionBuilder<TState, TEvent> State(TState state);
    public ParallelRegionDefinitionBuilder<TState, TEvent> Terminal(TState state);
}
```

### Behavioral Requirements

- Region-scoped `State` automatically assigns membership.
- Region-scoped `Terminal` marks both terminal state and region membership.
- Region-scoped transitions still use existing transition semantics.
- Duplicate membership remains invalid if users mix APIs incorrectly.

### Validation Requirements

- Diagnostics should suggest using region-scoped builders when membership is missing or duplicated.
- Region builder should prevent blank names early when possible.

### Test Plan

- Region-scoped initial state definition.
- Region-scoped terminal state definition.
- Region-scoped transition definition.
- Mixed old/new API compatibility.
- Duplicate membership validation remains effective.

### Acceptance Criteria

- Common two-region definitions become shorter and less error-prone.
- Existing fluent API remains source compatible.

---

## 8. Dedicated Parallel Regions Guide and Samples

### Summary

Create comprehensive user-facing documentation and samples for parallel regions.

### Problem

Parallel regions are a complex modeling feature. Users need a focused guide rather than scattered notes in general FSM, graph, and release documentation.

### Goals

- Add a dedicated documentation page for parallel regions.
- Add a standalone sample project if warranted.
- Cover valid patterns, invalid patterns, dispatch, completion, graph export, and limitations.
- Include migration guidance from flattened state combinations.

### Non-Goals

- No separate hosted workflow sample.
- No image rendering sample beyond existing visualization adapters.

### Proposed Documentation Structure

```text
docs/examples/parallel-regions.md
samples/Core.ParallelRegionsSample/
```

Documentation sections:

- What orthogonal regions are.
- When to use parallel regions.
- When not to use them.
- Defining a parallel composite.
- Region initial and terminal states.
- Same-event multi-region dispatch.
- Conflict examples.
- Completion examples.
- Active-state shape introspection.
- Graph export and visualization adapters.
- Persistence/history limitations or follow-up features.
- Migration from flattened state combinations.

### Sample Requirements

- Two-region order processing example.
- Events that advance one region.
- Shared event that advances both regions.
- Completion only after all regions terminal.
- Graph export with region metadata.
- Console output suitable for release tests.

### Test Plan

- Release test verifies sample runs.
- Release test verifies docs mention out-of-scope boundaries.
- Release test verifies graph metadata appears in sample output.

### Acceptance Criteria

- Users can implement a two-region model from docs alone.
- Docs clearly state unsupported workflow/persistence/history boundaries.
- Release validation covers the guide and sample.
