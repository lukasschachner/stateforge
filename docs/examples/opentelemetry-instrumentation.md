# OpenTelemetry instrumentation

`StateForge.OpenTelemetry` provides an observer adapter that emits activities and metrics from Core transition observations. Core does not reference telemetry packages.

```csharp
using StateForge.OpenTelemetry;

using var observer = new StateMachineTelemetryObserver<OrderState, OrderEvent>();
var runtime = definition.CreateRuntime(OrderState.Created, observer: observer);
await runtime.ApplyAsync(new SubmitOrder());
```

Stable names:

- Activity source: `StateForge.OpenTelemetry`
- Meter: `StateForge.OpenTelemetry`
- Activity: `state_machine.transition`
- Counter: `state_machine.transition.attempts`
- Histogram: `state_machine.transition.duration`

Common attributes include machine name (`state_machine.name`, when `StateMachineMetadataKeys.Name` metadata is set), source state, target state, resulting state, event type, transition kind, outcome, committed flag, lifecycle phase, and attempt id. Exception details are recorded only when `RecordExceptionDetails` is enabled.

The adapter does **not** configure exporters, processors, dependency injection registration, hosted services, logging, or startup behavior. Applications register the source and meter names in their own telemetry pipeline.

Run the sample:

```bash
dotnet run --project samples/OpenTelemetry.InstrumentationSample/OpenTelemetry.InstrumentationSample.csproj
```
