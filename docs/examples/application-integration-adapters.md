# Application integration adapters

The optional DependencyInjection and Logging packages keep Core free of application-framework dependencies while providing composition helpers for applications.

## Dependency injection

Register immutable definitions by name or by typed state/event shape:

```csharp
services.AddStateMachines(machines =>
{
    machines.AddDefinition("checkout", checkoutDefinition, machine => machine.ValidateOnStartup());
    machines.AddDefinition<CheckoutState, CheckoutEvent>(checkoutDefinition);
});
```

Resolve a factory and choose runtime ownership details explicitly:

```csharp
var factory = provider.GetRequiredService<IStateMachineRuntimeFactoryResolver>()
    .GetFactory<CheckoutState, CheckoutEvent>("checkout");
var runtime = factory.Create(CheckoutState.Cart, ConcurrencyMode.Serialized);
```

Startup validation is explicit and creates no runtime instances:

```csharp
var result = await provider.GetRequiredService<IStateMachineRegistrationValidator>().ValidateAsync();
```

## Logging

Create a safe structured logging observer from an `ILogger`:

```csharp
var options = StateMachineLoggingExtensions.AddStateMachineLogging(o => o.UseDefaultSafeDiagnostics());
var observer = logger.CreateStateMachineLoggingObserver<CheckoutState, CheckoutEvent>(options);
```

Default diagnostic projection uses stable identifiers and reason codes. It does not serialize raw event payloads, stack traces, environment data, or secret-like values.

## Persistence coordination

Persistent registrations accept application-supplied provider-neutral coordination. The adapter does not choose a database, serializer, storage connection, or provider lifetime.
