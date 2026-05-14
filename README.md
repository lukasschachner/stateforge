# StateForge

A dependency-light .NET finite state machine library for defining typed states and events, validating definitions,
applying transitions with explicit outcomes, observing transition lifecycles, optionally emitting
OpenTelemetry-compatible instrumentation, and inspecting graph data. The repository currently produces release-candidate
NuGet artifacts for Core, SourceGenerators, Persistence, and OpenTelemetry instrumentation.

## What the library is

StateForge is a plain finite state machine toolkit for .NET. It provides strongly typed definitions,
async-first transition execution, validation findings, explicit transition outcomes, definition introspection, and graph
export data that callers can adapt to their own tools.

## What the library is not

It is not a workflow engine. It does not include workflow orchestration, event sourcing, hosted services, dependency
injection integrations, dependency injection registration, exporter setup, built-in database persistence providers, or
visualization rendering in Core. Hierarchical states and opt-in parallel states/regions with history restore are Core
FSM modeling features; they do not add background scheduling or provider-specific persistence.

## Package selection and installation

Choose only the packages needed by your application:

| Package                                      | Use when                                                                                                                                                               |
|----------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `StateForge.Core`                   | You need fluent finite state machine definitions, validation, runtime execution, outcomes, permitted-event queries, introspection, and graph data.                     |
| `StateForge.SourceGenerators`       | You want optional build-time declaration syntax that generates Core definitions. It is an analyzer/source-generator package and is not a runtime replacement for Core. |
| `StateForge.Persistence`            | You need provider-neutral persistence contracts and apply-and-persist coordination while keeping storage in application-owned code.                                    |
| `StateForge.OpenTelemetry`          | You want optional OpenTelemetry-compatible activities and metrics from Core transition observations while owning all exporter and pipeline setup.                      |
| `StateForge.Visualization.Mermaid`  | You want deterministic Mermaid state diagram text from exported Core definition graphs.                                                                                |
| `StateForge.Visualization.Graphviz` | You want deterministic Graphviz DOT text from exported Core definition graphs.                                                                                         |
| `StateForge.Visualization.PlantUML` | You want deterministic PlantUML state diagram text from exported Core definition graphs.                                                                               |

```bash
dotnet add package StateForge.Core --prerelease
dotnet add package StateForge.SourceGenerators --prerelease
dotnet add package StateForge.Persistence --prerelease
dotnet add package StateForge.OpenTelemetry --prerelease
dotnet add package StateForge.Visualization.Mermaid --prerelease
dotnet add package StateForge.Visualization.Graphviz --prerelease
dotnet add package StateForge.Visualization.PlantUML --prerelease
```

## First fluent finite state machine

```csharp
using StateForge.Core.Definitions;
using StateForge.Core.Execution;

enum OrderState { Created, Paid, Shipped, Cancelled }
abstract record OrderEvent;
record Pay(decimal Amount) : OrderEvent;
record Ship(string TrackingNumber) : OrderEvent;
record Cancel(string Reason) : OrderEvent;

var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
{
    builder.State(OrderState.Created)
        .On<Pay>()
            .When(ctx => ((Pay)ctx.Event).Amount > 0, "positive payment")
            .GoTo(OrderState.Paid)
        .On<Cancel>()
            .GoTo(OrderState.Cancelled);

    builder.State(OrderState.Paid)
        .On<Ship>()
            .GoTo(OrderState.Shipped);

    builder.State(OrderState.Shipped).Terminal();
    builder.State(OrderState.Cancelled).Terminal();
});

var current = OrderState.Created;
var outcome = await definition.ApplyAsync(current, new Pay(42m));
if (outcome.Category == TransitionOutcomeCategory.Success)
{
    current = outcome.ResultingState;
}
```

See the runnable sample at [`samples/Core.FluentSample`](samples/Core.FluentSample).

## Optional source generation

`StateForge.SourceGenerators` provides attribute/DSL declarations that generate the same Core definition contract (`Definition` and `CreateDefinition`) plus additive event helpers and renderer-neutral `GeneratedMetadata`/`GeneratedGraph` records for tests and documentation. The generator reports stable `SMG###` diagnostics for statically knowable declaration mistakes and keeps Roslyn dependencies private to the analyzer package.

See [`docs/examples/source-generation.md`](docs/examples/source-generation.md) and [`samples/SourceGenerators.Sample`](samples/SourceGenerators.Sample).

## Optional hierarchical states

Core can model nested states without changing flat FSM behavior. A composite state declares an `InitialChild`, child
states call `ChildOf`, transitions from active leaves are tried before parent fallback transitions, and
`TransitionOutcome.ActiveStatePath` reports the resulting path from root to active leaf.

```csharp
builder.State(DocumentState.Draft)
    .On<Submit>().GoTo(DocumentState.Reviewing);
builder.State(DocumentState.Reviewing)
    .InitialChild(DocumentState.AuthorReview)
    .On<Cancel>().GoTo(DocumentState.Rejected);
builder.State(DocumentState.AuthorReview)
    .On<Submit>().GoTo(DocumentState.LegalReview);
builder.State(DocumentState.LegalReview)
    .ChildOf(DocumentState.Reviewing);

var outcome = await definition.ApplyAsync(DocumentState.Draft, new Submit());
Console.WriteLine(outcome.ActiveStatePath); // Reviewing -> AuthorReview
```

Hierarchy remains deterministic and Core-only: ordinary hierarchical composites have one active leaf, and opt-in
parallel composites have one active leaf per declared region. Core still has no workflow orchestration, background
scheduling, persistence provider, or rendering concepts. See [
`samples/Core.HierarchySample`](samples/Core.HierarchySample) and [
`docs/examples/core-fsm.md`](docs/examples/core-fsm.md).

## Parallel history restore

Parallel composites can opt into direct shallow or deep history. Runtime instances record the last active leaf/path per
owned region, restore a complete region shape when re-entering through history, and fall back to each region's
configured initial state when that region has no recorded history.

```csharp
builder.ParallelComposite(OrderState.Operational)
    .WithHistory(HistoryMode.Shallow)
    .Region("Fulfillment", OrderState.WaitingForPick, OrderState.Packing)
    .Region("Billing", OrderState.WaitingForPayment, OrderState.CapturingPayment);

var runtime = definition.CreateRuntime(OrderState.Operational);
await runtime.ApplyAsync(OrderEvent.AdvanceFulfillment);
foreach (var snapshot in runtime.ParallelHistorySnapshots)
{
    Console.WriteLine($"{snapshot.CompositeState}: {snapshot.HistoryMode} entries={snapshot.RegionEntries.Count}");
}
```

Parallel history is provider-neutral and renderer-neutral. Snapshot data is separate from current `ActiveStateShape`;
graph export exposes descriptive history metadata for tooling. It does not add event sourcing, checkpoint scheduling,
database providers, hosted services, or concurrent regional action execution. For a dedicated orthogonal-region walkthrough,
see [`docs/examples/parallel-regions.md`](docs/examples/parallel-regions.md).

## Active state snapshots

Core can capture the current active shape as a provider-neutral `ActiveStateSnapshot<TState>` and restore a runtime from
that snapshot after validation. Snapshot kinds cover flat `SingleLeaf`, ordered `Hierarchical` paths, and declaration-
ordered `Parallel` region entries.

```csharp
var snapshot = runtime.CaptureActiveStateSnapshot();
var validation = definition.ValidateActiveStateSnapshot(snapshot);
if (validation.IsValid)
{
    var restored = definition.CreateRuntime(snapshot);
}
```

The abstraction is additive: existing `CreateRuntime(initialState)` and single-state persistence flows remain available,
while hierarchical and parallel adopters can store active paths and region shapes with application-owned serialization.

## Transition preview and denial diagnostics

Use `PreviewAsync` to ask what an event would do from a supplied `ActiveStateShape<TState>` or from a runtime without
committing state. Preview reports the selected transition, guard diagnostics, expected direct target/shape when knowable,
and structured `TransitionDenialReason` data for denied previews. It does not run entry/exit actions, transition actions,
transition behaviors, observers, persistence hooks, telemetry hooks, or completion cascades. Guard predicates may execute,
so guards used with preview should be pure/idempotent.

```csharp
var preview = await definition.PreviewAsync(
    ActiveStateShape<OrderState>.Single(OrderState.Created),
    new Pay(42m),
    cancellationToken);

if (preview.IsPermitted)
{
    Console.WriteLine($"Would reach {preview.ExpectedTargetState}");
}

var denied = await runtime.ApplyAsync(new Ship("TRACK-123"), cancellationToken);
foreach (var diagnostic in denied.DenialDiagnostics)
{
    Console.WriteLine(diagnostic.Reason);
}
```

See [`docs/examples/core-fsm.md`](docs/examples/core-fsm.md) for permitted previews, guard-denied previews, actual denied
attempt diagnostics, and the side-effect-free preview caveats.

## Core lifecycle actions

States can declare `OnExit`/`OnExitAsync` and `OnEntry`/`OnEntryAsync` actions, and transitions can declare `Execute`/
`ExecuteAsync` actions. For permitted transitions, Core runs source exit actions, transition actions, and target entry
actions before committing the resulting state; completed/outcome observer notifications happen after commit. If an
action throws or observes cancellation before commit, remaining actions are skipped and the source state is preserved.
Definitions without actions keep existing behavior.

Action summaries are safe for validation, introspection, graph export, and visualization rendering; those paths never
execute user action delegates. See [`docs/examples/core-actions.md`](docs/examples/core-actions.md) and [
`samples/Core.ActionsSample`](samples/Core.ActionsSample).

## Advanced state-transfer example (Offer → Order → Invoice → Invoice Cancellation)

`Core.FluentSample` also demonstrates a multi-stage process with explicit state transfer between offer, order, invoice,
and invoice-cancellation states, including a reversible cancellation branch:

- `OfferDraft -> OfferSent -> OrderCreated`
- `OrderCreated -> OrderConfirmed -> InvoiceDraft -> InvoiceIssued`
- `InvoiceIssued -> InvoiceCancellationRequested -> InvoiceIssued` (rejected cancellation)
- `InvoiceIssued -> InvoiceCancellationRequested -> InvoiceCancelled` (approved cancellation)

Run it:

```bash
dotnet run --project samples/Core.FluentSample/Core.FluentSample.csproj --configuration Release
```

## Interactive API + frontend showcase sample

The repository includes an interactive sample that hosts a state machine runtime behind an ASP.NET Core API and renders
runtime/introspection data in a browser UI.

The sample demonstrates a more complex model with hierarchy, parallel regions, guarded transitions, completion, and
history restore while still staying in Core finite-state-machine semantics.

Run it:

```bash
dotnet run --project samples/Interactive.ApiFrontendSample/Interactive.ApiFrontendSample.csproj --configuration Release
```

Then open the URL printed by ASP.NET Core and use the UI to preview/apply events against the runtime.

See [`docs/examples/interactive-api-frontend-sample.md`](docs/examples/interactive-api-frontend-sample.md).

## Transition observation

Core exposes a dependency-free observer contract for deterministic transition lifecycle notifications:

```csharp
using StateForge.Core.Execution;

public sealed class RecordingObserver<TState, TEvent> : ITransitionObserver<TState, TEvent>
{
    public List<TransitionObservation<TState, TEvent>> Observations { get; } = new();

    public ValueTask ObserveAsync(TransitionObservation<TState, TEvent> observation, CancellationToken cancellationToken = default)
    {
        Observations.Add(observation);
        return ValueTask.CompletedTask;
    }
}

var observer = new RecordingObserver<OrderState, OrderEvent>();
var runtime = definition.CreateRuntime(OrderState.Created, observer: observer);
await runtime.ApplyAsync(new Pay(42m));
```

Successful transitions emit `Started -> Committed -> Completed -> Outcome`; denied, failed, cancelled,
validation-failed, and not-permitted attempts emit their documented terminal notification followed by exactly one final
`Outcome`. Observer exceptions and cancellations are isolated from transition outcomes. If no observer is supplied,
existing no-observer behavior is preserved. Use `CompositeTransitionObserver<TState,TEvent>` to fan out to multiple
observers and `FilteredTransitionObserver<TState,TEvent>` to forward only selected notification kinds. Set
`StateMachineMetadataKeys.Name` to attach a stable logical machine name to observations.

See [`samples/Core.ObservationSample`](samples/Core.ObservationSample) and [
`docs/examples/transition-observation.md`](docs/examples/transition-observation.md).

## OpenTelemetry instrumentation

`StateForge.OpenTelemetry` adapts Core observations into OpenTelemetry-compatible traces and metrics without
adding telemetry dependencies to Core:

```csharp
using StateForge.OpenTelemetry;

using var observer = new StateMachineTelemetryObserver<OrderState, OrderEvent>();
var runtime = definition.CreateRuntime(OrderState.Created, observer: observer);
await runtime.ApplyAsync(new Pay(42m));
```

The adapter emits activity source and meter name `StateForge.OpenTelemetry`, activity
`state_machine.transition`, counter `state_machine.transition.attempts`, and histogram
`state_machine.transition.duration`. If `StateMachineMetadataKeys.Name` is configured, telemetry includes
`state_machine.name`. Consumers are responsible for registering sources/meters, samplers, processors, exporters, hosting
integration, and any dependency injection registration.

See [`samples/OpenTelemetry.InstrumentationSample`](samples/OpenTelemetry.InstrumentationSample) and [
`docs/examples/opentelemetry-instrumentation.md`](docs/examples/opentelemetry-instrumentation.md).

## Source-generator example

The optional source-generator package lets you declare a machine and consume the generated Core definition:

```csharp
using StateForge.SourceGeneration;

[StateMachine(typeof(OrderState), typeof(OrderEvent))]
[State(OrderState.Created)]
[State(OrderState.Paid)]
[Event(OrderEvent.Pay)]
[Transition(OrderState.Created, OrderEvent.Pay, OrderState.Paid)]
public static partial class GeneratedOrderMachine { }

var definition = GeneratedOrderMachine.Definition;
```

The generator also supports optional advanced declarations for hierarchy, history, terminal states, and parallel composites with named regions. Existing flat declarations remain valid and Core/fluent definitions continue to work unchanged. Generated definitions still use Core builder calls and therefore validate, execute, introspect, and export graph metadata like equivalent fluent definitions.

See [`samples/SourceGenerators.Sample`](samples/SourceGenerators.Sample) and [`docs/examples/source-generators.md`](docs/examples/source-generators.md).

## Graph export and introspection example

Core exposes structured graph data and remains renderer-neutral.

```csharp
var export = definition.ExportGraph();
foreach (var edge in export.Graph!.Edges)
{
    Console.WriteLine($"{edge.SourceNodeId} --{edge.Label}--> {edge.TargetNodeId}");
}
```

Runtime instances can also export the same static graph with an additive active-state overlay:

```csharp
var runtimeGraph = runtime.ExportGraph().Graph!;
Console.WriteLine(runtimeGraph.RuntimeOverlay!.ActiveLeafState);
```

The overlay is renderer-neutral, side-effect-free, and includes active paths or declaration-ordered parallel region status when those features are used. Set `RuntimeGraphExportOptions.OverlayMode` to `None` to keep `RuntimeOverlay` null.

See [`samples/Graph.IntrospectionSample`](samples/Graph.IntrospectionSample) and [
`docs/examples/graph-introspection.md`](docs/examples/graph-introspection.md).

## Optional graph rendering adapters

Install any renderer package independently and convert exported graph data into deterministic text diagrams:

```csharp
using StateForge.Visualization.Graphviz.Rendering;
using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.PlantUML.Rendering;

var graph = definition.ExportGraph().Graph!;
var mermaid = MermaidGraphRenderer.Render(graph);
var dot = GraphvizDotRenderer.Render(graph);
var puml = PlantUmlGraphRenderer.Render(graph);
```

Renderers ignore runtime overlays by default. Enable adapter hints with `RenderRuntimeOverlay = true` to emit deterministic active-state comments/classes from graph data only.

Renderers produce text only (`.mmd`, `.dot`, `.puml`) and do not execute transitions, invoke browser tooling, call
Graphviz/PlantUML runtimes, or generate images. See [`samples/Graph.RenderingSample`](samples/Graph.RenderingSample)
and [`docs/examples/graph-rendering.md`](docs/examples/graph-rendering.md).

## Provider-neutral persistence example

Persistence uses application-owned storage through contracts such as `IStateSnapshotStore<TState>`:

```csharp
var runtime = PersistentStateMachineRuntime<OrderState, OrderEvent>.Create(definition, "order-1", store);
var persisted = await runtime.ApplyAndPersistAsync(new Submit());
```

The package does not ship database providers or retry policies. See [
`samples/Persistence.Sample`](samples/Persistence.Sample) and [
`docs/examples/persistence.md`](docs/examples/persistence.md).

## Release validation and contributor package readiness

Release-candidate validation is local and CI-friendly. It restores, builds, tests, verifies formatting, packs, and
inspects artifacts without publishing them:

```bash
./eng/validate-release.sh
# or
pwsh ./eng/validate-release.ps1
```

Publishing to NuGet.org or any registry is intentionally out of scope for this repository flow. See [
`docs/release-readiness.md`](docs/release-readiness.md) for public API snapshot and package-boundary workflows.

### History-enabled hierarchical states

Hierarchical composites can opt into shallow history with `WithShallowHistory()`. Re-entering the composite restores the
last committed active child, or an explicit/initial fallback when no record exists. History is in-memory per runtime
instance; persistence providers, event sourcing, workflow orchestration, and parallel regions remain out of scope for
Core.

## Parallel regions (orthogonal composites)

Core definitions can opt into parallel-region semantics for a composite state. A parallel composite owns named regions,
each with an initial state and exactly one active leaf at runtime. Dispatch evaluates regions deterministically in
declaration order, can advance multiple independent regions for the same event, and rejects invalid sibling-region
boundary transitions before committing state.

Region-scoped block syntax keeps membership, initial states, terminal states, and regional transitions colocated:

```csharp
builder.ParallelComposite(OrderState.Operational, composite =>
{
    composite.Region("Fulfillment", region =>
    {
        region.Initial(OrderState.WaitingForPick)
            .On(OrderEvent.PickStarted)
            .GoTo(OrderState.Packing);
        region.Terminal(OrderState.FulfillmentDone);
    });

    composite.Region("Billing", region =>
    {
        region.Initial(OrderState.WaitingForPayment);
        region.Terminal(OrderState.BillingDone);
    });
});
```

Existing `Region(...)` and `ParallelRegion(...)` declarations remain valid; the block APIs are additive and populate the
same validation, runtime, introspection, and graph-export model.

Parallel regions stay inside the Core FSM boundary: they do not add workflow orchestration, hosted services, persistence
providers, event sourcing, image rendering, or a concurrent regional action scheduler. Graph export exposes
renderer-neutral region metadata for optional visualization adapters.

### Structured transition conflict diagnostics

Validation results and non-success transition outcomes expose additive `ConflictDiagnostics` collections for tooling that
needs stable categories without parsing readable summary text. Diagnostics use `TransitionConflictKind` values such as
`DuplicateSourceScope`, `ParentRegionalConflict`, `CrossRegionBoundary`, `InvalidPostShape`, and `CompletionConflict`,
plus ordered participants with transition IDs aligned to graph edge IDs where available. Existing validation findings and
`TransitionDiagnostics.Summary` remain available for human-readable compatibility.

### Completion transitions

Composite states can route automatically when they complete without inventing a user event:

```csharp
builder.State(OrderState.Reviewing)
    .InitialChild(OrderState.AuthorReview)
    .OnCompletion()
    .GoTo(OrderState.Approved);
```

Completion is recognized after terminal entry actions succeed. Parallel composites complete only when all declared regions are terminal. Completion transitions support guards, transition actions, validation diagnostics, observer trigger metadata, and graph export classification via `GraphTriggerKind.Completion`.

### Optional application integration adapters

- `StateForge.DependencyInjection` registers named or typed definitions, runtime factories, observers, explicit startup validation, and provider-neutral persistence coordination.
- `StateForge.Logging` projects Core observations and validation findings into safe structured logs using `Microsoft.Extensions.Logging.Abstractions`.

See `docs/examples/application-integration-adapters.md` and `samples/ApplicationIntegration.Sample`.
