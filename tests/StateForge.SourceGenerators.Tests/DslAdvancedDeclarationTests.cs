namespace StateForge.SourceGenerators.Tests;

public sealed class DslAdvancedDeclarationTests
{
    [Fact]
    public void DslSupportsMetadataSelfInternalConditionAndBehaviorDeclarations()
    {
        var source = """
                     using StateForge.Core.Execution;
                     using StateForge.SourceGeneration;
                     public enum S { A, B }
                     public enum E { Stay, Ignore, Go }
                     [StateMachine(typeof(S), typeof(E))]
                     public static partial class M
                     {
                         private static void Define(StateMachineDeclaration<S,E> machine)
                         {
                             machine.WithMetadata("owner", "ops");
                             machine.State(S.A).WithMetadata("label", "alpha")
                                 .On(E.Stay).When("Can").Execute("Do").Self()
                                 .On(E.Ignore).Internal()
                                 .On(E.Go).GoTo(S.B);
                         }
                         private static bool Can(TransitionContext<S,E> context) => true;
                         private static void Do(TransitionContext<S,E> context) { }
                     }
                     """;
        var result = GeneratorTestHost.Run(source);
        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("machine.WithMetadata(\"owner\", \"ops\")", result.GeneratedSource);
        Assert.Contains(".WithMetadata(\"label\", \"alpha\")", result.GeneratedSource);
        Assert.Contains(".Self()", result.GeneratedSource);
        Assert.Contains(".Internal()", result.GeneratedSource);
        Assert.Contains(".When(Can).Execute(Do)", result.GeneratedSource);
    }

    [Fact]
    public void DslSupportsHierarchyAndParallelRegionDeclarations()
    {
        var source = """
                     using StateForge.SourceGeneration;
                     public enum S { Root, A, B, Operational, Pick, PickDone, Pay, PayDone }
                     public enum E { Next }
                     [StateMachine(typeof(S), typeof(E))]
                     public static partial class M
                     {
                         private static void Define(StateMachineDeclaration<S,E> machine)
                         {
                             machine.State(S.Root).InitialChild(S.A).WithShallowHistory(S.A);
                             machine.State(S.B).ChildOf(S.Root).Terminal();
                             machine.State(S.Operational)
                                 .ParallelComposite()
                                 .Region("Fulfillment", S.Pick)
                                 .Member(S.PickDone).Terminal()
                                 .Region("Billing", S.Pay)
                                 .Member(S.PayDone).Terminal();
                         }
                     }
                     """;

        var result = GeneratorTestHost.Run(source);

        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains("machine.State(S.Root).InitialChild(S.A).WithHistory(global::StateForge.Core.Definitions.HistoryMode.Shallow, S.A);", result.GeneratedSource);
        Assert.Contains("machine.State(S.B).ChildOf(S.Root).Terminal();", result.GeneratedSource);
        var fulfillmentRegion = "machine.ParallelComposite(S.Operational).Region(\"Fulfillment\", S.Pick, new global::S[] { S.PickDone }, new global::S[] { S.PickDone });";
        var billingRegion = "machine.ParallelComposite(S.Operational).Region(\"Billing\", S.Pay, new global::S[] { S.PayDone }, new global::S[] { S.PayDone });";
        Assert.Contains(fulfillmentRegion, result.GeneratedSource);
        Assert.Contains(billingRegion, result.GeneratedSource);
        Assert.True(result.GeneratedSource.IndexOf(fulfillmentRegion, StringComparison.Ordinal) <
                    result.GeneratedSource.IndexOf(billingRegion, StringComparison.Ordinal));
    }

}
