namespace StateForge.SourceGenerators.Tests;

public sealed class GeneratedEventHelperRuntimeTests
{
    [Fact]
    public void HelpersPreserveDirectRuntimeAndDefinitionApplySemanticsInGeneratedSource()
    {
        var result = GeneratorTestHost.Run(GeneratedErgonomicsTestSources.SimpleMachine);
        GeneratedSourceAssertions.ContainsInOrder(result.GeneratedSource,
            "public static global::System.Threading.Tasks.ValueTask<global::StateForge.Core.Execution.TransitionOutcome<global::S, global::E>> ApplyE_GoAsync(global::StateForge.Core.Execution.StateMachineRuntime<global::S, global::E> runtime",
            "return runtime.ApplyAsync(E.Go, cancellationToken);",
            "public static global::System.Threading.Tasks.ValueTask<global::StateForge.Core.Execution.TransitionOutcome<global::S, global::E>> ApplyE_GoAsync(global::S currentState",
            "return Definition.ApplyAsync(currentState, E.Go, cancellationToken);");
    }

    [Fact]
    public void HelpersForwardCancellationTokenToCoreApplyPaths()
    {
        var result = GeneratorTestHost.Run(GeneratedErgonomicsTestSources.SimpleMachine);
        Assert.Contains("cancellationToken", result.GeneratedSource);
        Assert.Contains("runtime.ApplyAsync(E.Go, cancellationToken)", result.GeneratedSource);
        Assert.Contains("Definition.ApplyAsync(currentState, E.Go, cancellationToken)", result.GeneratedSource);
    }
}
