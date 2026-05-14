namespace StateForge.SourceGenerators.Tests;

public sealed class AsyncMemberReferenceGenerationTests
{
    [Fact]
    public void AsyncConditionAndBehaviorReferencesUseAsyncCoreBuilderMethods()
    {
        var source = """
                     using System.Threading;
                     using System.Threading.Tasks;
                     using StateForge.Core.Execution;
                     using StateForge.SourceGeneration;
                     public enum S { A, B }
                     public enum E { Go }
                     [StateMachine(typeof(S), typeof(E))]
                     public static partial class M
                     {
                         private static void Define(StateMachineDeclaration<S,E> machine) =>
                             machine.State(S.A).On(E.Go).When("CanGoAsync").Execute("MoveAsync").GoTo(S.B);

                         private static ValueTask<bool> CanGoAsync(TransitionContext<S,E> context, CancellationToken cancellationToken) => ValueTask.FromResult(true);
                         private static ValueTask MoveAsync(TransitionContext<S,E> context, CancellationToken cancellationToken) => ValueTask.CompletedTask;
                     }
                     """;
        var result = GeneratorTestHost.Run(source);

        GeneratorTestHost.AssertCompiles(result);
        Assert.Contains(".WhenAsync(CanGoAsync).ExecuteAsync(MoveAsync).GoTo(S.B)", result.GeneratedSource);
    }
}