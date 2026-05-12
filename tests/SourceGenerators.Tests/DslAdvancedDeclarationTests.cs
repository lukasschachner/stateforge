namespace StateMachineLibrary.SourceGenerators.Tests;

public sealed class DslAdvancedDeclarationTests
{
    [Fact]
    public void DslSupportsMetadataSelfInternalConditionAndBehaviorDeclarations()
    {
        var source = """
                     using StateMachineLibrary.Core.Execution;
                     using StateMachineLibrary.SourceGeneration;
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
}