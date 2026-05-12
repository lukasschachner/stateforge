namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class DslUnsupportedSyntaxDiagnosticTests
{
    [Fact]
    public void UnsupportedDslLoopsBranchesAndHelpersProduceDiagnostics()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public enum S { A, B }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     public static partial class M
                     {
                         private static void Define(StateMachineDeclaration<S,E> machine)
                         {
                             if (true) machine.State(S.A);
                             Helper(machine);
                         }
                         private static void Helper(StateMachineDeclaration<S,E> machine) { }
                     }
                     """;
        var result = GeneratorTestHost.Run(source);
        Assert.NotEmpty(result.GeneratorDiagnostics("SMG005"));
    }
}