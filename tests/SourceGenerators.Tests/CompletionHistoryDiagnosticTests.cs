using Microsoft.CodeAnalysis;

namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class CompletionHistoryDiagnosticTests
{
    [Fact]
    public void ReportsHistoryFallbackThatBelongsToAnotherParent()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public enum S { Root, Other, A }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Root, History = HistoryMode.Shallow, HistoryFallback = S.A)]
                     [State(S.Other)]
                     [State(S.A, Parent = S.Other)]
                     public static partial class M { }
                     """;

        var diagnostic = GeneratorDiagnosticAssertions.AssertDiagnostic(
            GeneratorTestHost.Run(source), "SMG014", DiagnosticSeverity.Error, "history fallback");
        Assert.Contains("child", diagnostic.GetMessage());
    }
}
