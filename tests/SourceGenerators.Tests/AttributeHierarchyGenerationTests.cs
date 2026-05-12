namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class AttributeHierarchyGenerationTests
{
    [Fact]
    public void AttributeDeclarationsEmitHierarchyHistoryAndTerminalBuilderCalls()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public enum S { Root, A, B, Done }
                     public enum E { Next }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Root, InitialChild = S.A, History = HistoryMode.Shallow, HistoryFallback = S.A)]
                     [State(S.A, Parent = S.Root)]
                     [State(S.B, Parent = S.Root)]
                     [State(S.Done, Parent = S.Root, IsTerminal = true)]
                     [Event(E.Next)]
                     [Transition(S.A, E.Next, S.Done)]
                     public static partial class M { }
                     """;

        var result = GeneratorTestHost.Run(source);

        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("machine.State(S.Root).InitialChild(S.A).WithHistory(global::StateMachineLibrary.Core.Definitions.HistoryMode.Shallow, S.A);", result.GeneratedSource);
        Assert.Contains("machine.State(S.A).ChildOf(S.Root);", result.GeneratedSource);
        Assert.Contains("machine.State(S.Done).ChildOf(S.Root).Terminal();", result.GeneratedSource);
        Assert.Contains("machine.State(S.A).On(E.Next).GoTo(S.Done);", result.GeneratedSource);
    }
}
