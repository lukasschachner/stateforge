# Core FSM Example

The Core APIs remain the primary manual definition surface. The optional source-generator package can create equivalent `StateMachineDefinition<TState,TEvent>` instances from declarations, but it does not replace or change these manual APIs. The optional Persistence package builds on these Core contracts and does not change non-persistent Core usage.

```csharp
using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Core.Introspection;

enum OrderState { Created, Paid, Shipped, Cancelled }

abstract record OrderEvent;
record Pay(decimal Amount) : OrderEvent;
record Ship(string TrackingNumber) : OrderEvent;
record Cancel(string Reason) : OrderEvent;

var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
{
    builder.State(OrderState.Created)
        .On<Pay>()
            .WhenAsync(async (ctx, ct) =>
            {
                await Task.Yield();
                ct.ThrowIfCancellationRequested();
                return ((Pay)ctx.Event).Amount > 0;
            }, "positive payment")
            .GoTo(OrderState.Paid)
        .On<Cancel>()
            .GoTo(OrderState.Cancelled);

    builder.State(OrderState.Paid)
        .On<Ship>()
            .GoTo(OrderState.Shipped)
        .On<Cancel>()
            .GoTo(OrderState.Cancelled);

    builder.State(OrderState.Shipped).Terminal();
    builder.State(OrderState.Cancelled).Terminal();
});

var validation = definition.Validate();
if (!validation.IsValid)
{
    foreach (var finding in validation.Findings)
    {
        Console.WriteLine($"{finding.Severity}: {finding.Code}: {finding.Message}");
    }
}
```

## Advanced multi-stage state transfer example

The runnable sample in `samples/Core.FluentSample` now includes a larger business flow with explicit transfer points:

- Offer: `OfferDraft -> OfferSent -> OfferRejected` (or accepted path)
- Order: `OrderCreated -> OrderConfirmed -> OrderCancelled`
- Invoice: `InvoiceDraft -> InvoiceIssued -> InvoicePaid`
- Invoice cancellation subprocess: `InvoiceIssued -> InvoiceCancellationRequested -> (Approved => InvoiceCancelled | Rejected => InvoiceIssued)`

Example transition setup shape:

```csharp
builder.State(CommercialState.OfferSent)
    .On<AcceptOffer>()
        .GoTo(CommercialState.OrderCreated)
    .On<RejectOffer>()
        .GoTo(CommercialState.OfferRejected);

builder.State(CommercialState.InvoiceIssued)
    .On<RegisterPayment>()
        .When(ctx => ((RegisterPayment)ctx.Event).Amount > 0, "payment amount must be positive")
        .GoTo(CommercialState.InvoicePaid)
    .On<RequestInvoiceCancellation>()
        .GoTo(CommercialState.InvoiceCancellationRequested);

builder.State(CommercialState.InvoiceCancellationRequested)
    .On<ApproveInvoiceCancellation>()
        .GoTo(CommercialState.InvoiceCancelled)
    .On<RejectInvoiceCancellation>()
        .GoTo(CommercialState.InvoiceIssued);
```

This shows state transfer between major process stages and a reversible cancellation branch that returns to `InvoiceIssued` when cancellation is rejected.

## Optional hierarchical modeling

Flat machines continue to work without hierarchy metadata. To opt in, declare a composite state with one initial child and mark nested states with `ChildOf`:

```csharp
enum DocumentState { Draft, Reviewing, AuthorReview, LegalReview, Approved, Rejected }
abstract record DocumentEvent;
record Submit : DocumentEvent;
record Approve : DocumentEvent;
record Cancel : DocumentEvent;

var definition = StateMachineDefinition<DocumentState, DocumentEvent>.Create(builder =>
{
    builder.State(DocumentState.Draft)
        .On<Submit>().GoTo(DocumentState.Reviewing);

    builder.State(DocumentState.Reviewing)
        .InitialChild(DocumentState.AuthorReview)
        .On<Cancel>().GoTo(DocumentState.Rejected); // parent fallback from active children

    builder.State(DocumentState.AuthorReview)
        .On<Submit>().GoTo(DocumentState.LegalReview);

    builder.State(DocumentState.LegalReview)
        .ChildOf(DocumentState.Reviewing)
        .On<Approve>().GoTo(DocumentState.Approved);

    builder.State(DocumentState.Approved).ChildOf(DocumentState.Reviewing).Terminal();
    builder.State(DocumentState.Rejected).Terminal();
});

var outcome = await definition.ApplyAsync(DocumentState.Draft, new Submit());
Console.WriteLine(outcome.ActiveLeafState);      // AuthorReview
Console.WriteLine(outcome.ActiveStatePath);      // Reviewing -> AuthorReview
```

Hierarchy semantics are deterministic: entering a composite resolves through initial children to one active leaf; transitions are resolved leaf-to-root so child transitions override parent fallback transitions; external hierarchy transitions exit from leaf to least common ancestor and enter down to the target leaf. Validation rejects missing parents, cycles, missing/invalid initial children, and same-level ambiguous transitions.

## Supplied-state execution

```csharp
var currentState = OrderState.Created;
var outcome = await definition.ApplyAsync(currentState, new Pay(42m), cancellationToken);

if (outcome.Category == TransitionOutcomeCategory.Success)
{
    currentState = outcome.ResultingState;
}
else
{
    Console.WriteLine(outcome.Diagnostics.Summary);
}
```

## State-owning runtime

```csharp
var runtime = definition.CreateRuntime(OrderState.Created, ConcurrencyMode.Serialized);
await runtime.ApplyAsync(new Pay(42m), cancellationToken);
await runtime.ApplyAsync(new Ship("TRACK-123"), cancellationToken);
Console.WriteLine(runtime.CurrentState);
```

## External-state runtime

```csharp
var accessor = StateAccessor.Create(
    get: () => currentState,
    set: state => currentState = state);

var externalRuntime = definition.CreateRuntime(accessor, ConcurrencyMode.Serialized);
await externalRuntime.ApplyAsync(new Cancel("customer request"), cancellationToken);
```

## Introspection

```csharp
var events = await definition.GetPermittedEventsAsync(OrderState.Created, cancellationToken);
var graphMetadata = definition.GetGraphMetadata();
var transitions = definition.Introspect().DeclaredTransitions;
```

## Graph export

```csharp
GraphExportResult<OrderState, OrderEvent> export = definition.ExportGraph();

if (!export.Succeeded)
{
    foreach (var finding in export.Validation.Findings)
    {
        Console.WriteLine($"{finding.Severity}: {finding.Code}: {finding.Message}");
    }

    return;
}

var graph = export.Graph!;
foreach (var node in graph.Nodes)
{
    Console.WriteLine($"{node.Id}: {node.Label} terminal={node.IsTerminal}");
}

foreach (var edge in graph.Edges)
{
    Console.WriteLine($"{edge.Id}: {edge.SourceNodeId} --{edge.Label}--> {edge.TargetNodeId}");
    Console.WriteLine($"event={edge.Event.DisplayName}; kind={edge.Kind}; conditions={edge.Conditions.DisplayText}");
}
```

Graph export is structured definition data only. The core library does not render diagrams, calculate layout, or reference visualization packages; optional adapters can consume `graph.Nodes`, `graph.Edges`, and metadata outside the core package.

## Behavior notes

- Outcomes are explicit: `Success`, `Denied`, `NotPermitted`, `ValidationFailure`, `Cancelled`, and `BehaviorFailure`.
- External/self transitions run condition, exit, transition, commit, then entry phases.
- Internal transitions run conditions and transition behavior without exit/entry behavior or state change.
- Denied, not-permitted, validation-failure, pre-commit cancellation, and pre-commit behavior failure preserve the previous state.
- Entry failures occur after the commit point, so `ResultingState` reports the committed state.
- `ConcurrencyMode.Fast` is caller-managed and avoids runtime serialization. `ConcurrencyMode.Serialized` serializes overlapping attempts per runtime context.

## Runnable sample

The same fluent runtime flow is validated by `samples/Core.FluentSample`. Run it with:

```bash
dotnet run --project samples/Core.FluentSample/Core.FluentSample.csproj --configuration Release
```

This sample uses typed states/events and async transition conditions. It does not require hosting, dependency injection, workflow orchestration, event sourcing, or parallel states. For hierarchy-specific usage, run `samples/Core.HierarchySample`.

## Lifecycle actions

For entry, exit, and transition actions, including ordering and failure semantics, see [core actions](core-actions.md).

## Hierarchy history states

Composite states can opt into in-memory history restoration. Use `WithShallowHistory()` on a composite to restore the last committed direct child when the composite is re-entered. If no record exists, the composite uses an explicit fallback child (`WithShallowHistory(fallbackChild)`) or its initial child.

```csharp
builder.State(DocumentState.Reviewing)
    .InitialChild(DocumentState.AuthorReview)
    .WithShallowHistory()
    .On<Pause>().GoTo(DocumentState.Suspended);

builder.State(DocumentState.Suspended)
    .On<Resume>().GoTo(DocumentState.Reviewing); // restores last review child
```

History records are runtime-instance local and are not persisted by Core. Failed, denied, or canceled transitions do not mutate history.

## Parallel-region composite example

Use `ParallelComposite(...).Region(...)` to model orthogonal parts of a composite state. Entering the composite activates one leaf per region, and a shared event can advance independent regions in declaration order.

```csharp
var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
{
    builder.ParallelComposite(OrderState.Operational)
        .Region("Fulfillment", OrderState.WaitingForPick, [OrderState.Packing])
        .Region("Billing", OrderState.WaitingForPayment, [OrderState.CapturingPayment]);

    builder.State(OrderState.WaitingForPick).On(OrderEvent.Advance).GoTo(OrderState.Packing);
    builder.State(OrderState.WaitingForPayment).On(OrderEvent.Advance).GoTo(OrderState.CapturingPayment);
});
```

Validation rejects zero regions, blank or duplicate region names, missing initial states, duplicate membership, direct sibling-region transitions, ambiguous source/event transitions, and unreachable regional states. Completion is considered true only when all regions are terminal.

## Parallel-history restore

Parallel composites can enable direct shallow or deep history. The owning composite records history per region and restores one effective active leaf per owned region when re-entered through history. Missing region history uses the region initial state as deterministic fallback.

```csharp
var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
{
    builder.State(OrderState.Idle).On(OrderEvent.Start).GoTo(OrderState.Operational);
    builder.State(OrderState.Operational).On(OrderEvent.Cancel).GoTo(OrderState.Cancelled);
    builder.State(OrderState.Cancelled).On(OrderEvent.Start).GoTo(OrderState.Operational);

    builder.ParallelComposite(OrderState.Operational)
        .WithHistory(HistoryMode.Deep) // or HistoryMode.Shallow
        .Region("Fulfillment", OrderState.WaitingForPick, OrderState.Packing)
        .Region("Billing", OrderState.WaitingForPayment, OrderState.CapturingPayment);
});

var runtime = definition.CreateRuntime(OrderState.Operational);
await runtime.ApplyAsync(OrderEvent.AdvanceFulfillment);
await runtime.ApplyAsync(OrderEvent.Cancel);
await runtime.ApplyAsync(OrderEvent.Start); // restores recorded fulfillment; missing billing falls back

foreach (var snapshot in runtime.ParallelHistorySnapshots)
{
    Console.WriteLine($"{snapshot.CompositeState} {snapshot.HistoryMode} complete={snapshot.HasCompleteRecordedShape}");
}
```

Parallel-history snapshots are runtime-instance local and provider-neutral. They are intentionally separate from `runtime.ActiveStateShape`, which reports current activity. Core validates supplied snapshot-like data with `ValidateParallelHistorySnapshot(...)` before it can be used by consumers.

## Active-state snapshots

Use `CaptureActiveStateSnapshot()` when an application needs a provider-neutral shape snapshot that can later restore a Core runtime. The snapshot kind distinguishes flat `SingleLeaf`, ordered `Hierarchical`, and multi-region `Parallel` active shapes.

```csharp
var runtime = definition.CreateRuntime(OrderState.Operational);
await runtime.ApplyAsync(OrderEvent.Advance);

var snapshot = runtime.CaptureActiveStateSnapshot();
var validation = definition.ValidateActiveStateSnapshot(snapshot);
if (validation.IsValid)
{
    var restored = definition.CreateRuntime(snapshot);
    Console.WriteLine(restored.CaptureActiveStateSnapshot().Kind);
}
```

Flat snapshots carry one active leaf and no region metadata. Hierarchical snapshots add an ordered root-to-leaf `ActivePath`. Parallel snapshots carry the owning composite plus one declaration-ordered `ActiveRegionSnapshot` per region, including region id/name, active leaf path, and terminal flag.

## Completion Transitions

Ordinary composite states can declare `OnCompletion()` to leave the composite automatically after the active child flow reaches a terminal state and that terminal state's entry actions complete successfully:

```csharp
builder.State(OrderState.Reviewing)
    .InitialChild(OrderState.AuthorReview)
    .OnCompletion()
    .GoTo(OrderState.Approved);

builder.State(OrderState.AuthorReview)
    .ChildOf(OrderState.Reviewing)
    .On(OrderEvent.AuthorApproves)
    .GoTo(OrderState.ReviewDone);

builder.State(OrderState.ReviewDone)
    .ChildOf(OrderState.Reviewing)
    .Terminal();
```

Parallel composites can also declare completion transitions. The transition is eligible only when every owned region is terminal. Completion transitions use completion trigger metadata rather than an artificial user event, run synchronously within transition processing, and are recognized at most once for each active scope episode; exiting and re-entering the scope creates a new episode.
