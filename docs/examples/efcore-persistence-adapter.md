# EF Core Persistence Adapter

`StateForge.Persistence.EntityFrameworkCore` provides an optional EF Core-backed implementation of `IStateSnapshotStore<TState>`.

## Responsibilities

The adapter is intentionally narrow:

- persists and loads `StateSnapshot<TState>` records;
- enforces expected-version optimistic concurrency;
- maps storage outcomes into `SnapshotLoadResult<TState>` / `SnapshotSaveResult<TState>`;
- keeps diagnostics safe by default.

The **application** still owns provider selection, `DbContext` lifetime, migrations, transactions, and retention.

## Quick start

```csharp
await using var context = new MyDbContext(options);
var store = new EntityFrameworkCoreSnapshotStore<MyState>(context,
    new StateForgeEntityFrameworkCoreOptions<MyState>
    {
        SnapshotSetResolver = c => ((MyDbContext)c).Snapshots
    });

var save = await store.SaveAsync(PersistenceVersion.From(0L), proposedSnapshot, ct);
var load = await store.LoadAsync(instanceId, definitionId, ct);
```

Use expected version `0` for explicit create semantics.

## Security notes

Default diagnostics avoid leaking payloads, connection strings, provider-internal exception text, and stack traces.

The adapter uses EF Core APIs only (no raw SQL string construction).
