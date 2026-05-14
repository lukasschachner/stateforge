namespace StateForge.SourceGenerators.Tests;

public sealed class NestedTypeGenerationTests
{
    [Fact]
    public void DeclarationsOnNestedPartialTypesGenerateInsideContainingPartialTypes()
    {
        var source = """
                     using StateForge.SourceGeneration;
                     namespace Demo;
                     public enum S { A, B }
                     public enum E { Go }
                     public static partial class Outer
                     {
                         [StateMachine(typeof(S), typeof(E))]
                         [State(S.A)]
                         [State(S.B)]
                         [Event(E.Go)]
                         [Transition(S.A, E.Go, S.B)]
                         public static partial class Machine { }
                     }
                     """;
        var result = GeneratorTestHost.Run(source);

        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("namespace Demo", result.GeneratedSource);
        Assert.Contains("public static partial class Outer", result.GeneratedSource);
        Assert.Contains("public static partial class Machine", result.GeneratedSource);
        Assert.Contains(".On(E.Go).GoTo(S.B)", result.GeneratedSource);
    }
}