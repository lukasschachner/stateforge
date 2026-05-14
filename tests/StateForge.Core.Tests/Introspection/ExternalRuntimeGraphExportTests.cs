using StateForge.Core.Execution;
using StateForge.Core.Introspection;

namespace StateForge.Core.Tests.Introspection;

public sealed class ExternalRuntimeGraphExportTests
{
    [Fact]
    public async Task External_runtime_export_reads_current_state_without_writing()
    {
        var writes = 0;
        var state = RuntimeGraphState.Created;
        var runtime = RuntimeGraphExportTestDomain.CreateFlatDefinition()
            .CreateRuntime(StateAccessor.Create(() => state, next =>
            {
                writes++;
                state = next;
            }));

        var overlay = RuntimeGraphExportAssertions.RuntimeOverlay(await runtime.ExportGraphAsync());

        Assert.Equal(RuntimeGraphState.Created, overlay.ActiveLeafState);
        Assert.Equal(0, writes);
    }

    [Fact]
    public async Task External_runtime_export_honors_accessor_cancellation()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateFlatDefinition()
            .CreateRuntime(StateAccessor.Create<RuntimeGraphState>(
                cancellationToken =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return ValueTask.FromResult(RuntimeGraphState.Created);
                },
                (_, _) => ValueTask.CompletedTask));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await runtime.ExportGraphAsync(new RuntimeGraphExportOptions(), cts.Token));
    }
}
