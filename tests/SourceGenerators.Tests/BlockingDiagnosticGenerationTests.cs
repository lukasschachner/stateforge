namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class BlockingDiagnosticGenerationTests
{
    [Fact]
    public void BlockingErrorsSuppressRunnableDefinitionSource()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public enum S { A }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.A)]
                     [Transition(S.A, E.Go, S.A)]
                     public static partial class M { public static object CreateDefinition() => new(); }
                     """;
        var result = GeneratorTestHost.Run(source);
        Assert.NotEmpty(result.GeneratorDiagnostics("SMG007"));
        Assert.DoesNotContain(result.GeneratedHintNames,
            h => h.EndsWith(".StateMachine.g.cs", StringComparison.Ordinal));
    }
}