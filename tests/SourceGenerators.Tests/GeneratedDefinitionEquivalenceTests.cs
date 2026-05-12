namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class GeneratedDefinitionEquivalenceTests
{
    [Fact]
    public void GeneratedDefinitionUsesSameCoreBuilderAndValidationSurfaceAsManualDefinitions()
    {
        var result = GeneratorTestHost.Run(TestSources.AttributeLifecycle);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("StateMachineDefinition<global::OrderState, global::OrderEvent>.Create(machine =>",
            result.GeneratedSource);
        Assert.Contains("machine.State(OrderState.Created)", result.GeneratedSource);
    }

    [Fact]
    public void GeneratedAdvancedDefinitionsUseCoreHierarchyAndRegionBuilderSurface()
    {
        var source = """
                     using StateMachineLibrary.SourceGeneration;
                     public enum S { Root, A, Operational, R1, RDone }
                     public enum E { Finish }
                     [StateMachine(typeof(S), typeof(E))]
                     [State(S.Root, InitialChild = S.A, History = HistoryMode.Deep)]
                     [State(S.A, Parent = S.Root)]
                     [State(S.Operational, IsParallelComposite = true)]
                     [Region(S.Operational, "R", S.R1, IsInitial = true)]
                     [Region(S.Operational, "R", S.RDone, IsTerminal = true)]
                     [Event(E.Finish)]
                     [Transition(S.R1, E.Finish, S.RDone)]
                     public static partial class M { }
                     """;

        var result = GeneratorTestHost.Run(source);

        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains(".InitialChild(S.A).WithHistory(global::StateMachineLibrary.Core.Definitions.HistoryMode.Deep)", result.GeneratedSource);
        Assert.Contains(".ParallelComposite()", result.GeneratedSource);
        Assert.Contains("machine.ParallelComposite(S.Operational).Region(\"R\", S.R1, new global::S[] { S.RDone }, new global::S[] { S.RDone });", result.GeneratedSource);
        Assert.Contains("machine.State(S.RDone).ChildOf(S.Operational).Terminal();", result.GeneratedSource);
        Assert.Contains("machine.State(S.R1).On(E.Finish).GoTo(S.RDone);", result.GeneratedSource);
    }
}
