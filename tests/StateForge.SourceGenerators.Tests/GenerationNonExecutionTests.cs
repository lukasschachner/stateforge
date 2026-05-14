namespace StateForge.SourceGenerators.Tests;

public sealed class GenerationNonExecutionTests
{
    [Fact]
    public void DeclarationMethodConditionAndBehaviorAreReferencedButNotExecutedDuringGeneration()
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
                             machine.State(S.A).On(E.Go).When("CanGo").Execute("Move").GoTo(S.B);
                         private static bool CanGo(TransitionContext<S,E> context) => throw new System.Exception("not during generation");
                         private static void Move(TransitionContext<S,E> context) => throw new System.Exception("not during generation");
                     }
                     """;
        var result = GeneratorTestHost.Run(source);
        GeneratorTestHost.AssertCompiles(result);
    }
}