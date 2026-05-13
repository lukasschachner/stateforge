# Source generation

The source-generator package can build a Core definition from declarative attributes or the compact declaration DSL.

```csharp
using StateMachineLibrary.SourceGeneration;

[StateMachine(typeof(OrderState), typeof(OrderEvent))]
[State(OrderState.Created)]
[State(OrderState.Paid, IsTerminal = true)]
[Event(OrderEvent.Pay)]
[Transition(OrderState.Created, OrderEvent.Pay, OrderState.Paid)]
public static partial class OrderMachine;
```

Generated members remain additive and keep the existing contract:

- `Definition`
- `CreateDefinition()`
- generated event helpers such as `ApplyOrderEvent_PayAsync(...)` when helper generation is safe
- `GeneratedMetadata`
- `GeneratedGraph`

Diagnostics use stable `SMG###` IDs for duplicate declarations, missing references, invalid terminal transitions, invalid advanced declarations, and conservative static graph issues. Diagnostics are deterministic and avoid environment-specific values.

Generated metadata and graph entries are renderer-neutral string records intended for tests and documentation. They do not require Mermaid, Graphviz, PlantUML, browser tooling, network access, or renderer packages.
