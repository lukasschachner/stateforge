# Source Generator Examples

The optional `StateMachineLibrary.SourceGenerators` analyzer package lets a consumer declare finite state machine definitions at compile time. Generated code still builds `StateMachineDefinition<TState,TEvent>` from the Core package, so validation, introspection, graph export, and runtime behavior remain unchanged.

## Attribute declaration

```csharp
using StateMachineLibrary.SourceGeneration;

public enum OrderState { Created, Paid, Shipped, Cancelled }
public enum OrderEvent { Pay, Ship, Cancel }

[StateMachine(typeof(OrderState), typeof(OrderEvent))]
[State(OrderState.Created)]
[State(OrderState.Paid)]
[State(OrderState.Shipped, IsTerminal = true)]
[State(OrderState.Cancelled, IsTerminal = true)]
[Event(OrderEvent.Pay)]
[Event(OrderEvent.Ship)]
[Event(OrderEvent.Cancel)]
[Transition(OrderState.Created, OrderEvent.Pay, OrderState.Paid)]
[Transition(OrderState.Paid, OrderEvent.Ship, OrderState.Shipped)]
public static partial class OrderMachine { }
```

Use the generated members:

```csharp
var definition = OrderMachine.Definition;
var freshDefinition = OrderMachine.CreateDefinition();
```

## Compact DSL declaration

```csharp
using StateMachineLibrary.SourceGeneration;

[StateMachine(typeof(OrderState), typeof(OrderEvent))]
public static partial class OrderMachineDsl
{
    private static void Define(StateMachineDeclaration<OrderState, OrderEvent> machine)
    {
        machine.State(OrderState.Created)
            .On(OrderEvent.Pay).GoTo(OrderState.Paid);

        machine.State(OrderState.Paid)
            .On(OrderEvent.Ship).GoTo(OrderState.Shipped);

        machine.State(OrderState.Shipped).Terminal();
    }
}
```

The DSL method is parsed, not executed. Use only recognized declaration calls with statically resolvable values.

## Hierarchy and parallel regions

Advanced declaration syntax can express the same hierarchy, history, terminal, and parallel-region metadata as the Core fluent builder. Attribute declarations remain explicit about events:

```csharp
using StateMachineLibrary.SourceGeneration;

public enum OrderState { Draft, Operational, Pick, PickDone, Pay, PayDone }
public enum OrderEvent { Start, Picked, Paid }

[StateMachine(typeof(OrderState), typeof(OrderEvent))]
[State(OrderState.Draft)]
[State(OrderState.Operational, IsParallelComposite = true)]
[Region(OrderState.Operational, "Fulfillment", OrderState.Pick, IsInitial = true)]
[Region(OrderState.Operational, "Fulfillment", OrderState.PickDone, IsTerminal = true)]
[Region(OrderState.Operational, "Billing", OrderState.Pay, IsInitial = true)]
[Region(OrderState.Operational, "Billing", OrderState.PayDone, IsTerminal = true)]
[Event(OrderEvent.Start)]
[Event(OrderEvent.Picked)]
[Event(OrderEvent.Paid)]
[Transition(OrderState.Draft, OrderEvent.Start, OrderState.Operational)]
[Transition(OrderState.Pick, OrderEvent.Picked, OrderState.PickDone)]
[Transition(OrderState.Pay, OrderEvent.Paid, OrderState.PayDone)]
public static partial class GeneratedParallelOrderMachine { }
```

The compact DSL supports equivalent marker calls:

```csharp
machine.State(OrderState.Operational)
    .ParallelComposite()
    .Region("Fulfillment", OrderState.Pick)
    .Member(OrderState.PickDone).Terminal()
    .Region("Billing", OrderState.Pay)
    .Member(OrderState.PayDone).Terminal();
```

Generated code calls Core builder APIs such as `InitialChild`, `WithHistory`, `ParallelComposite`, and `Region`; it does not add renderer-specific behavior or execute user code during generation.

## Payload events

```csharp
public interface IOrderEvent;
public sealed record Pay(decimal Amount) : IOrderEvent;

[StateMachine(typeof(OrderState), typeof(IOrderEvent))]
[State(OrderState.Created)]
[State(OrderState.Paid)]
[Event(typeof(Pay))]
[Transition(OrderState.Created, typeof(Pay), OrderState.Paid)]
public static partial class PayloadOrderMachine { }
```

## Conditions and behaviors

```csharp
using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.SourceGeneration;

[StateMachine(typeof(OrderState), typeof(OrderEvent))]
public static partial class GuardedOrderMachine
{
    private static void Define(StateMachineDeclaration<OrderState, OrderEvent> machine) =>
        machine.State(OrderState.Created)
            .On(OrderEvent.Pay)
            .When("CanPay", "payment accepted")
            .Execute("RecordPayment")
            .GoTo(OrderState.Paid);

    private static bool CanPay(TransitionContext<OrderState, OrderEvent> context) => true;
    private static void RecordPayment(TransitionContext<OrderState, OrderEvent> context) { }
}
```

The generator verifies referenced member signatures where practical and emits method-group references; it never evaluates these members during generation.

## Diagnostics

The generator reports blocking diagnostics for duplicate states/events, missing transition endpoints, ambiguous transitions, terminal states with outgoing transitions, unsupported DSL syntax, invalid member references, generated member name conflicts, duplicate explicit parallel regions, missing regional initials, duplicate sibling region membership, unsupported history modes, and invalid advanced role combinations. Fix diagnostics before relying on `Definition`.

## Runnable sample

A runnable source-generator declaration sample lives in `samples/SourceGenerators.Sample`:

```bash
dotnet run --project samples/SourceGenerators.Sample/SourceGenerators.Sample.csproj --configuration Release
```

The sample references `StateMachineLibrary.SourceGenerators` as an analyzer and consumes the generated Core definition at runtime.
