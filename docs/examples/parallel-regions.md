# Parallel Regions Guide

Orthogonal regions, also called parallel regions, let one finite-state-machine composite state contain multiple independent active regions. In Core this is FSM modeling semantics: a parallel composite has one active leaf per region, one event dispatch can evaluate enabled transitions in those regions, and the resulting active-state shape is updated deterministically. Parallel regions are not thread-level concurrency, background work, workflow orchestration, hosted services, event sourcing, persistence providers, or image rendering.

Run the validated sample:

```bash
dotnet run --project samples/Core.ParallelRegionsSample/Core.ParallelRegionsSample.csproj --configuration Release
```

The sample prints stable labels including `Active regions:`, `Completion status before all terminal:`, `Completion status after all terminal:`, `Graph region:`, `Invalid model diagnostic:`, and `Parallel regions sample completed`.

## What orthogonal regions are

A parallel composite is a parent state that owns named regions. Entering the composite initializes each region to its configured initial state, so the runtime active-state shape contains one active leaf per active region instead of a single active leaf for the whole machine. The owning composite remains the logical state scope while the regions represent independent dimensions of that scope.

For an order-processing model, the parent state can be `Operational` while the `Fulfillment` and `Billing` regions progress independently:

- `Fulfillment`: `WaitingForPick -> Packing -> FulfillmentDone`
- `Billing`: `WaitingForPayment -> CapturingPayment -> BillingDone`

## When to use parallel regions

Use parallel regions when a single logical state contains independent dimensions that can progress without exploding into flattened state combinations. Good fits include:

- an order that is operational while fulfillment and billing each have their own regional progress;
- UI or protocol modes where independent sub-modes should be active at the same time;
- models where shared events sometimes advance multiple independent dimensions in one dispatch;
- tools that need active-state shape or `DefinitionGraph.Regions` metadata for documentation and diagnostics.

## When not to use parallel regions

Do not use parallel regions as a replacement for execution concurrency, queues, schedulers, or distributed workflow infrastructure. They do not create threads, hosted services, background workers, event sourcing, persistence providers, or image rendering. If the main problem is durable workflow orchestration, application-owned persistence, retry scheduling, process hosting, or external service coordination, keep those concerns outside Core and use the state machine as the deterministic FSM model inside that larger design.

## Defining a parallel composite

Declare the parent with `ParallelComposite` and add named regions. Region-scoped `Initial`, `State`, and `Terminal` calls assign membership and return the existing state builder, so ordinary transition behavior is unchanged.

```csharp
var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
{
    builder.ParallelComposite(OrderState.Operational, composite =>
    {
        composite.Region("Fulfillment", region =>
        {
            region.Initial(OrderState.WaitingForPick)
                .On(OrderEvent.PickStarted)
                .GoTo(OrderState.Packing);
            region.State(OrderState.Packing)
                .On(OrderEvent.CompleteOrder)
                .GoTo(OrderState.FulfillmentDone);
            region.Terminal(OrderState.FulfillmentDone);
        });

        composite.Region("Billing", region =>
        {
            region.Initial(OrderState.WaitingForPayment)
                .On(OrderEvent.PaymentStarted)
                .GoTo(OrderState.CapturingPayment);
            region.State(OrderState.CapturingPayment)
                .On(OrderEvent.CompleteOrder)
                .GoTo(OrderState.BillingDone);
            region.Terminal(OrderState.BillingDone);
        });
    });
});
```

## Region initial states and terminal states

Every region needs one effective initial state. Entering `Operational` activates `WaitingForPick` in `Fulfillment` and `WaitingForPayment` in `Billing`. A region terminal state marks that region complete, but the composite is complete only when every active region is terminal.

The sample demonstrates this rule with labelled output. Before both regions are terminal, completion is false; after the shared terminal event advances both independent regions, completion is true:

```text
Completion status before all terminal: Operational complete=False
Completion status after all terminal: Operational complete=True
```

## Region declaration order and deterministic behavior

Region declarations are stable and deterministic. Core preserves declaration order for region definitions, active-state snapshots, graph export metadata, and multi-region dispatch processing. The guide and sample use stable region names and labelled output so release tests can assert behavior without relying on timestamps, process IDs, machine names, local paths, or random identifiers.

## Single-region dispatch

A single-region dispatch is an event that is enabled in one active region and has no enabled transition in the other active regions. In the order example, `PickStarted` advances only fulfillment:

```text
Fulfillment: WaitingForPick -> Packing
Billing: WaitingForPayment remains active
```

Likewise, `PaymentStarted` advances only billing while fulfillment remains in its current state.

## Same-event multi-region dispatch

A same-event multi-region dispatch occurs when the same event is enabled in multiple independent regions. One dispatch may advance multiple independent, non-conflicting regions; it is still one deterministic FSM dispatch, not concurrent regional action scheduling.

In the sample, `CompleteOrder` is enabled from both `Packing` and `CapturingPayment`, so one dispatch advances both regions to terminal states:

```text
Fulfillment: Packing -> FulfillmentDone
Billing: CapturingPayment -> BillingDone
```

## Completion after all regions are terminal

Completion after all regions are terminal means the owning parallel composite is considered complete only after every region has reached a terminal state. If only `Fulfillment` reaches `FulfillmentDone` while `Billing` remains in `CapturingPayment`, the composite is not complete. Completion transitions declared with `OnCompletion()` are eligible only after the all-regions-terminal condition is true.

## Conflict examples and diagnostics

Invalid model diagnostic examples should be fixed at definition/validation time. Common invalid patterns include:

- duplicate or invalid region names, such as blank names or duplicate sibling names;
- missing initial states for a declared region;
- illegal boundaries, such as direct sibling-region transitions that would move from a state in one region into a state owned by another region;
- ambiguous handling, such as multiple unguarded transitions for the same source/event scope;
- same-event conflicts where selected transitions cannot produce one valid post-dispatch active shape.

Conflicts are detected before commit. A conflicting dispatch must not be documented or treated as a successful partial regional commit; the previous active-state shape is preserved when the outcome is non-committed. Validation and runtime diagnostics use deterministic codes/messages that describe declared state, event, region, or transition identifiers without stack traces, local paths, environment values, secrets, or callback internals.

## Active-state shape introspection

Runtime active-state shape tells you what is currently active. For a parallel composite, `runtime.ActiveStateShape` is parallel and contains one declaration-ordered active region entry per region:

```csharp
foreach (var region in runtime.ActiveStateShape.ActiveRegions)
{
    Console.WriteLine($"{region.RegionName}: {region.ActiveLeafState} terminal={region.IsTerminal}");
}
```

This active-state shape is separate from history snapshots and graph data. Use it when you need current execution state for diagnostics, tests, or application-owned persistence serialization.

## Graph export and optional visualization adapters

Core graph export is renderer-neutral. `definition.ExportGraph().Graph!.Regions` exposes `DefinitionGraph.Regions` metadata such as region name, owning composite, initial state, member states, terminal states, and declaration order. The parallel-regions sample prints this data with the `Graph region:` label and does not require Mermaid, Graphviz, PlantUML, browser tools, native renderer binaries, hosted rendering services, or image rendering.

```text
Graph region: Fulfillment owner=Operational initial=WaitingForPick ...
Graph region: Billing owner=Operational initial=WaitingForPayment ...
```

Optional visualization adapters can consume the same `DefinitionGraph` data and render deterministic text formats. For adapter details, see [graph rendering](graph-rendering.md). For deeper graph and runtime overlay concepts, see [graph introspection](graph-introspection.md).

## Persistence and history limitations

Core exposes provider-neutral active-state shape, active-state snapshots, and runtime-local parallel-history snapshots. Core does not add database persistence providers, event sourcing, hosted checkpoint scheduling, serialization format ownership, or workflow orchestration. If you persist parallel state, serialize the active-state snapshot or region entries in application-owned storage and validate restored snapshots before creating a runtime. See [persistence](persistence.md) for provider-neutral persistence contracts and [core FSM](core-fsm.md) for active-state snapshot examples.

## Migration from flattened state combinations

Flattened state combinations encode independent dimensions as many combined states, for example `WaitingForPickAndWaitingForPayment`, `PackingAndWaitingForPayment`, and `PackingAndCapturingPayment`. Migrate only when the dimensions are truly independent and can be represented as regional states under one owning composite.

| Flattened combination | Fulfillment region | Billing region |
| --- | --- | --- |
| `WaitingForPickAndWaitingForPayment` | `WaitingForPick` | `WaitingForPayment` |
| `PackingAndWaitingForPayment` | `Packing` | `WaitingForPayment` |
| `PackingAndCapturingPayment` | `Packing` | `CapturingPayment` |
| `FulfillmentDoneAndBillingDone` | `FulfillmentDone` | `BillingDone` |

Use regional transitions for events that affect one independent dimension, such as `PickStarted` or `PaymentStarted`. Use shared events when one domain event should advance multiple independent regions in the same dispatch and those transitions do not conflict. Keep workflow orchestration, hosted services, event sourcing, persistence providers, and image rendering outside the migration; those boundaries are not solved by changing flattened states into parallel regions.

## Explicit out-of-scope boundaries

This guide and sample cover existing Core finite-state-machine behavior only. They do not add or promise:

- workflow orchestration;
- hosted services;
- event sourcing;
- persistence providers;
- image rendering;
- thread-level concurrency or background scheduling;
- external service calls or retry infrastructure.

## Related links

- [Core FSM example](core-fsm.md)
- [Graph introspection example](graph-introspection.md)
- [Graph rendering example](graph-rendering.md)
- [Persistence example](persistence.md)
