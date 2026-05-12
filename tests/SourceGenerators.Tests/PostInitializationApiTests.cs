namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class PostInitializationApiTests
{
    [Fact]
    public void GeneratorProvidedDeclarationApisCompile()
    {
        var result = GeneratorTestHost.Run(TestSources.DslLifecycle);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("StateMachineDeclarationApi.g.cs", result.GeneratedHintNames);
    }

    [Fact]
    public void GeneratorProvidedAdvancedDeclarationApisCompile()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public enum S { Root, A, Parallel, R1 }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Root, InitialChild = S.A, History = HistoryMode.Shallow, HistoryFallback = S.A)]
                     [State(S.A, Parent = S.Root)]
                     [ParallelComposite(S.Parallel)]
                     [ParallelRegion(S.Parallel, "R")]
                     [Region(S.Parallel, "R", S.R1, IsInitial = true, IsTerminal = true)]
                     public static partial class AdvancedApiMachine
                     {
                         private static void Define(StateMachineDeclaration<S,E> machine)
                         {
                             machine.State(S.Root).InitialChild(S.A).WithHistory(HistoryMode.Shallow, S.A);
                             machine.State(S.Parallel).ParallelComposite().Region("R", S.R1).Member(S.R1).Terminal();
                         }
                     }
                     """;

        var result = GeneratorTestHost.Run(source);

        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("internal enum HistoryMode", result.GeneratedSource);
        Assert.Contains("internal sealed class RegionAttribute", result.GeneratedSource);
    }

}
