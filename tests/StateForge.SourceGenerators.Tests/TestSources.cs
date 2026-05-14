namespace StateForge.SourceGenerators.Tests;

internal static class TestSources
{
    public const string AttributeLifecycle = """
                                             using StateForge.SourceGeneration;
                                             public enum OrderState { Created, Paid, Shipped, Cancelled }
                                             public enum OrderEvent { Pay, Ship, Cancel }
                                             [StateMachine(typeof(OrderState), typeof(OrderEvent))]
                                             [Metadata("owner", "sales")]
                                             [State(OrderState.Created)]
                                             [State(OrderState.Paid)]
                                             [State(OrderState.Shipped, IsTerminal = true)]
                                             [State(OrderState.Cancelled, IsTerminal = true)]
                                             [Event(OrderEvent.Pay)]
                                             [Event(OrderEvent.Ship)]
                                             [Event(OrderEvent.Cancel)]
                                             [Transition(OrderState.Created, OrderEvent.Pay, OrderState.Paid)]
                                             [Transition(OrderState.Created, OrderEvent.Cancel, OrderState.Cancelled)]
                                             [Transition(OrderState.Paid, OrderEvent.Ship, OrderState.Shipped)]
                                             public static partial class OrderMachine { }
                                             """;

    public const string DslLifecycle = """
                                       using StateForge.SourceGeneration;
                                       public enum OrderState { Created, Paid, Shipped, Cancelled }
                                       public enum OrderEvent { Pay, Ship, Cancel }
                                       [StateMachine(typeof(OrderState), typeof(OrderEvent))]
                                       public static partial class OrderMachineDsl
                                       {
                                           private static void Define(StateMachineDeclaration<OrderState, OrderEvent> machine)
                                           {
                                               machine.State(OrderState.Created)
                                                   .On(OrderEvent.Pay).GoTo(OrderState.Paid)
                                                   .On(OrderEvent.Cancel).GoTo(OrderState.Cancelled);
                                               machine.State(OrderState.Paid).On(OrderEvent.Ship).GoTo(OrderState.Shipped);
                                               machine.State(OrderState.Shipped).Terminal();
                                               machine.State(OrderState.Cancelled).Terminal();
                                           }
                                       }
                                       """;
}