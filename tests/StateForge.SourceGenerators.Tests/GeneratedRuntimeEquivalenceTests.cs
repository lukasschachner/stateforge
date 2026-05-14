namespace StateForge.SourceGenerators.Tests;

public sealed class GeneratedRuntimeEquivalenceTests
{
    [Fact]
    public void GeneratedRuntimePathKeepsConditionAndBehaviorReferencesForCoreExecution()
    {
        var source = """
                     using StateForge.Core.Execution;
                     using StateForge.SourceGeneration;
                     public enum S { A, B }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     public static partial class M
                     {
                         private static void Define(StateMachineDeclaration<S,E> machine) =>
                             machine.State(S.A).On(E.Go).When("CanGo").OnExit("Exit").Execute("Move").OnEntry("Enter").GoTo(S.B);
                         private static bool CanGo(TransitionContext<S,E> context) => true;
                         private static void Exit(TransitionContext<S,E> context) { }
                         private static void Move(TransitionContext<S,E> context) { }
                         private static void Enter(TransitionContext<S,E> context) { }
                     }
                     """;
        var result = GeneratorTestHost.Run(source);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains(".When(CanGo)", result.GeneratedSource);
        Assert.Contains(".OnExit(Exit).Execute(Move).OnEntry(Enter).GoTo(S.B)", result.GeneratedSource);
    }
}