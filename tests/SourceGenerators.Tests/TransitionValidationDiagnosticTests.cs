namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class TransitionValidationDiagnosticTests
{
    [Fact]
    public void AmbiguousAndTerminalOutgoingTransitionsProduceDiagnostics()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public enum S { A, B }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.A, IsTerminal = true)]
                     [State(S.B)]
                     [Event(E.Go)]
                     [Transition(S.A, E.Go, S.B)]
                     [Transition(S.A, E.Go, S.B)]
                     public static partial class M { }
                     """;
        var result = GeneratorTestHost.Run(source);
        Assert.NotEmpty(result.GeneratorDiagnostics("SMG003"));
        Assert.NotEmpty(result.GeneratorDiagnostics("SMG004"));
    }
}