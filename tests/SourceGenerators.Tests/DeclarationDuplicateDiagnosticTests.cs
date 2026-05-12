namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class DeclarationDuplicateDiagnosticTests
{
    [Fact]
    public void DuplicateStateEventAndGeneratedNameConflictsProduceDiagnostics()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public enum S { A, B }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.A)]
                     [State(S.A)]
                     [Event(E.Go)]
                     [Event(E.Go)]
                     public static partial class M { public static object Definition => new(); }
                     """;
        var result = GeneratorTestHost.Run(source);
        Assert.NotEmpty(result.GeneratorDiagnostics("SMG001"));
        Assert.NotEmpty(result.GeneratorDiagnostics("SMG007"));
    }
}