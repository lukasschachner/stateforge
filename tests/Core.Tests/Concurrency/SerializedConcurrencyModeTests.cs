using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Concurrency;

public class SerializedConcurrencyModeTests
{
    [Fact]
    public async Task SerializedModePreventsOverlappingAttemptsOnOneRuntime()
    {
        var current = 0;
        var max = 0;
        var definition = StateMachineDefinition<OrderState, OrderEvent>.Create(builder =>
        {
            builder.State(OrderState.Created).On<Pay>()
                .ExecuteAsync(async (_, ct) =>
                {
                    var now = Interlocked.Increment(ref current);
                    max = Math.Max(max, now);
                    await Task.Delay(50, ct);
                    Interlocked.Decrement(ref current);
                })
                .Self();
        });
        var runtime = definition.CreateRuntime(OrderState.Created, ConcurrencyMode.Serialized);

        await Task.WhenAll(runtime.ApplyAsync(new Pay()).AsTask(), runtime.ApplyAsync(new Pay()).AsTask());

        Assert.Equal(1, max);
    }
}