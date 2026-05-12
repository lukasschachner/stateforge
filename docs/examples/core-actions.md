# Core actions

Core definitions can attach lifecycle actions to states and transitions:

- `OnExit` / `OnExitAsync` on a state runs before an external or self transition leaves that state.
- `Execute` / `ExecuteAsync` on a transition registers transition actions.
- `OnEntry` / `OnEntryAsync` on a state runs before an external or self transition commits into that state.

Runtime order is deterministic: source exit actions, transition actions, target entry actions, state commit, then completed/outcome observer notifications.

```csharp
var log = new List<string>();
var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
{
    builder.State(OrderState.Created)
        .OnExit(_ => log.Add("exit Created"), "leave Created")
        .On<Pay>()
            .Execute(_ => log.Add("transition Pay"), "record payment")
            .GoTo(OrderState.Paid);

    builder.State(OrderState.Paid)
        .OnEntry(_ => log.Add("entry Paid"), "enter Paid");
});
```

If an action throws or observes cancellation before commit, remaining actions are skipped, the outcome is non-success, and the source state remains current. Validation, permitted-event queries, introspection, graph export, and renderers expose only action summaries and never invoke action delegates.

Run the sample:

```bash
dotnet run --project samples/Core.ActionsSample/Core.ActionsSample.csproj --configuration Release
```
