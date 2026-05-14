namespace StateForge.SourceGenerators.Tests;

public sealed class MemberReferenceDiagnosticTests
{
    [Fact]
    public void InvalidConditionAndBehaviorSignaturesProduceDiagnostics()
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
                             machine.State(S.A).On(E.Go).When("BadCondition").Execute("BadBehavior").GoTo(S.B);
                         private static int BadCondition(TransitionContext<S,E> context) => 1;
                         private static int BadBehavior(TransitionContext<S,E> context) => 1;
                     }
                     """;
        var result = GeneratorTestHost.Run(source);
        Assert.True(result.GeneratorDiagnostics("SMG006").Count() >= 2);
    }
}