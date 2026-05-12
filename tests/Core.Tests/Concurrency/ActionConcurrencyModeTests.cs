using Core.Tests.Actions;
using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Concurrency;

public class ActionConcurrencyModeTests
{
    [Fact]
    public async Task SerializedRuntimeSerializesLongRunningActions()
    {
        var concurrent = 0;
        var maxConcurrent = 0;
        var definition = StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created)
                .OnEntryAsync(async (_, _) =>
                {
                    var current = Interlocked.Increment(ref concurrent);
                    maxConcurrent = Math.Max(maxConcurrent, current);
                    await Task.Delay(20);
                    Interlocked.Decrement(ref concurrent);
                })
                .On<Stay>()
                .Self();
        });
        var runtime = definition.CreateRuntime(ActionState.Created, ConcurrencyMode.Serialized);

        await Task.WhenAll(runtime.ApplyAsync(new Stay()).AsTask(), runtime.ApplyAsync(new Stay()).AsTask());

        Assert.Equal(1, maxConcurrent);
    }

    [Fact]
    public async Task FastRuntimeStillAllowsCallerManagedConcurrency()
    {
        var definition = StateMachineDefinition<ActionState, ActionEvent>.Create(builder =>
        {
            builder.State(ActionState.Created)
                .OnEntryAsync(async (_, _) => await Task.Delay(1))
                .On<Stay>()
                .Self();
        });
        var runtime = definition.CreateRuntime(ActionState.Created);

        var outcomes = await Task.WhenAll(runtime.ApplyAsync(new Stay()).AsTask(),
            runtime.ApplyAsync(new Stay()).AsTask());

        Assert.All(outcomes, outcome => Assert.True(outcome.IsSuccess));
    }
}