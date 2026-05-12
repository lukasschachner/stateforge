namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class GeneratedAdvancedHierarchyRuntimeTests
{
    [Fact]
    public void GeneratedHierarchyDefinitionsCompileThroughCoreBuilderSurface()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public enum S { Root, A, Done }
                     public enum E { Finish }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Root, InitialChild = S.A, History = HistoryMode.Shallow)]
                     [State(S.A, Parent = S.Root)]
                     [State(S.Done, Parent = S.Root, IsTerminal = true)]
                     [Event(E.Finish)]
                     [Transition(S.A, E.Finish, S.Done)]
                     public static partial class M { }
                     """;

        var result = GeneratorTestHost.Run(source);

        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("StateMachineDefinition<global::S, global::E>.Create(machine =>", result.GeneratedSource);
        Assert.Contains(".On(E.Finish).GoTo(S.Done)", result.GeneratedSource);
    }
}
