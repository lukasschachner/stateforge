namespace StateForge.SourceGenerators.Tests;

public sealed class CompletionDeclarationTests
{
    [Fact]
    public void CompletionAttributeEmitsCompletionBuilderCallAndGraphEdge()
    {
        var source = """
                     using StateForge.SourceGeneration;
                     public enum S { Root, A, Done }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Root, InitialChild = S.A)]
                     [State(S.A, Parent = S.Root, IsTerminal = true)]
                     [State(S.Done, IsTerminal = true)]
                     [Completion(S.Root, S.Done)]
                     public static partial class M { }
                     """;

        var result = GeneratorTestHost.Run(source);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("machine.State(S.Root).OnCompletion().GoTo(S.Done);", result.GeneratedSource);
        Assert.Contains("edge:S.Root:Completion:S.Done", result.GeneratedSource);
    }
}
