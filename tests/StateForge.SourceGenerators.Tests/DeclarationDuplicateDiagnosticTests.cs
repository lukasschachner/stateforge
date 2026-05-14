namespace StateForge.SourceGenerators.Tests;

public sealed class DeclarationDuplicateDiagnosticTests
{
    [Fact]
    public void DuplicateStateEventTransitionAndGeneratedNameConflictsProduceDiagnostics()
    {
        var source = """
                     using StateForge.SourceGeneration;
                     public enum S { A, B }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.A)]
                     [State(S.B)]
                     [Event(E.Go)]
                     [Transition(S.A, E.Go, S.B)]
                     [Transition(S.A, E.Go, S.B)]
                     public static partial class M { }
                     """;
        var result = GeneratorTestHost.Run(source);
        Assert.NotEmpty(result.GeneratorDiagnostics("SMG003"));
    }
}