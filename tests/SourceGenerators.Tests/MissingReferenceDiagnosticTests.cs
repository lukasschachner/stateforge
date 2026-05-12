namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class MissingReferenceDiagnosticTests
{
    [Fact]
    public void MissingStateReferencesProduceDiagnostics()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public enum S { A, B }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.A)]
                     [Event(E.Go)]
                     [Transition(S.A, E.Go, S.B)]
                     public static partial class M { }
                     """;
        var result = GeneratorTestHost.Run(source);
        Assert.NotEmpty(result.GeneratorDiagnostics("SMG002"));
    }

    [Fact]
    public void MissingAttributeEventReferencesProduceDiagnostics()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public enum S { A, B }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.A)]
                     [State(S.B)]
                     [Transition(S.A, E.Go, S.B)]
                     public static partial class M { }
                     """;
        var result = GeneratorTestHost.Run(source);
        Assert.Contains(result.GeneratorDiagnostics("SMG002"), d => d.GetMessage().Contains("event"));
    }
}