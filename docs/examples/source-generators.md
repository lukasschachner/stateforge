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

The generator reports blocking diagnostics for duplicate states/events, missing transition endpoints, ambiguous transitions, terminal states with outgoing transitions, unsupported DSL syntax, invalid member references, and generated member name conflicts. Fix diagnostics before relying on `Definition`.

## Runnable sample

A runnable source-generator declaration sample lives in `samples/SourceGenerators.Sample`:

```bash
dotnet run --project samples/SourceGenerators.Sample/SourceGenerators.Sample.csproj --configuration Release
```

The sample references `StateMachineLibrary.SourceGenerators` as an analyzer and consumes the generated Core definition at runtime.
