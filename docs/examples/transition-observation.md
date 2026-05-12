# Transition observation

Core transition observation is explicit and dependency-free. Implement `ITransitionObserver<TState,TEvent>` and pass it to `ApplyAsync` or `CreateRuntime`. To distinguish multiple definitions in observations and telemetry, set the well-known `StateMachineMetadataKeys.Name` metadata key.

```csharp
builder.WithMetadata(StateMachineMetadataKeys.Name, "orders");

public sealed class RecordingObserver<TState, TEvent> : ITransitionObserver<TState, TEvent>
{
    public List<TransitionObservation<TState, TEvent>> Observations { get; } = new();

    public ValueTask ObserveAsync(TransitionObservation<TState, TEvent> observation, CancellationToken cancellationToken = default)
    {
        Observations.Add(observation);
        return ValueTask.CompletedTask;
    }
}
```

Ordering is deterministic:

- Success: `Started`, `Committed`, `Completed`, `Outcome`
- Condition denied: `Started`, `ConditionDenied`, `Outcome`
- Behavior failed: `Started`, optional `Committed`, `BehaviorFailed`, `Outcome`
- Cancelled: `Started`, optional `Committed`, `Cancelled`, `Outcome`
- Validation failure: `Started`, `ValidationFailure`, `Outcome`
- Not permitted: `Started`, `NotPermitted`, `Outcome`

Observer failures are suppressed by Core and do not change transition outcomes or diagnostics. Leaving the observer null preserves the existing no-observer execution path.

For multiple consumers, compose observers explicitly:

```csharp
var observer = new CompositeTransitionObserver<OrderState, OrderEvent>(
    customObserver,
    telemetryObserver);
```

To reduce noise, wrap an observer with a filter:

```csharp
var failuresOnly = new FilteredTransitionObserver<OrderState, OrderEvent>(
    customObserver,
    observation => observation.Kind is TransitionObservationKind.BehaviorFailed or TransitionObservationKind.Cancelled);
```

Run the sample:

```bash
dotnet run --project samples/Core.ObservationSample/Core.ObservationSample.csproj
```
