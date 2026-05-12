# Persistence (Optional)

This package is optional and provider-neutral. You keep ownership of storage.

## Reload and persist with an application-owned store

```csharp
var definition = PersistenceTestDomain.CreateDefinition();
var store = new ApplicationOwnedSampleStore();
store.Seed(new StateSnapshot<OrderState>("order-1", "orders-v1", OrderState.Draft, PersistenceVersion.From("v1")));

var load = await PersistentStateLoader.ReloadAsync(definition, "order-1", store);
if (load.Category == SnapshotLoadCategory.Loaded)
{
    var runtime = PersistentStateMachineRuntime<OrderState, OrderEvent>.Create(definition, "order-1", store);
    var result = await runtime.ApplyAndPersistAsync(new Submit());

    if (result.PersistenceCategory == TransitionPersistenceCategory.Persisted)
    {
        Console.WriteLine(result.CommittedSnapshot!.ActiveState);
    }
}
```

## Missing snapshot

```csharp
var missing = await PersistentStateLoader.ReloadAsync(definition, "missing", store);
if (missing.Category == SnapshotLoadCategory.MissingSnapshot)
{
    // Application chooses initialization strategy.
}
```

## Invalid snapshot

```csharp
// e.g., stored definition id or active state no longer matches definition
if (load.Category == SnapshotLoadCategory.InvalidSnapshot)
{
    Console.WriteLine(load.Diagnostics.Summary);
}
```

## Storage failure

```csharp
if (result.PersistenceCategory == TransitionPersistenceCategory.StorageFailure)
{
    // Proposed state was NOT committed.
}
```

## Concurrent state change

```csharp
if (result.PersistenceCategory == TransitionPersistenceCategory.ConcurrentStateChange)
{
    // Reload before retry; library does not retry automatically.
}
```

## Runnable sample

A provider-neutral persistence sample lives in `samples/Persistence.Sample`:

```bash
dotnet run --project samples/Persistence.Sample/Persistence.Sample.csproj --configuration Release
```

The sample implements application-owned in-memory storage behind `IStateSnapshotStore<TState>`. It intentionally avoids database providers, hosted services, dependency injection integrations, and automatic retry policies.

## Migrating to active-shape snapshots

Existing `StateSnapshot<TState>` persistence remains a single-active-state contract and is sufficient for flat or simple non-parallel machines. When an application needs hierarchical ancestry fidelity or parallel region fidelity, store the Core `ActiveStateSnapshot<TState>` payload with application-owned serialization and validate it before runtime creation:

```csharp
var activeSnapshot = runtime.CaptureActiveStateSnapshot();
// serialize activeSnapshot with application-owned storage

var validation = definition.ValidateActiveStateSnapshot(activeSnapshot);
if (!validation.IsValid)
{
    foreach (var diagnostic in validation.Diagnostics)
    {
        Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message}");
    }
    return;
}

var restored = definition.CreateRuntime(activeSnapshot);
```

This migration is additive: storage-level optimistic concurrency metadata can coexist with snapshot `Sequence`, and legacy single-state records can continue to load while new records include active-path or parallel-region details.
