using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;
using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Execution;

public sealed class TransitionPreviewGuardEvaluationTests
{
    [Fact]
    public async Task PreviewReportsGuardCancellation()
    {
        var definition = StateMachineDefinition<PreviewState, PreviewEvent>.Create(builder =>
        {
            builder.State(PreviewState.Draft).On<PreviewSubmit>().WhenAsync((_, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return ValueTask.FromResult(true);
            }, "cancel guard").GoTo(PreviewState.Review);
            builder.State(PreviewState.Review);
        });
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var preview = await definition.PreviewAsync(ActiveStateShape<PreviewState>.Single(PreviewState.Draft),
            new PreviewSubmit(), cts.Token);

        Assert.Equal(TransitionPreviewStatus.Cancelled, preview.Status);
        preview.AssertDenied(TransitionDenialReason.GuardEvaluationCancelled);
    }

    [Fact]
    public async Task PreviewReportsGuardFailureAsDiagnosticResult()
    {
        var definition = StateMachineDefinition<PreviewState, PreviewEvent>.Create(builder =>
        {
            builder.State(PreviewState.Draft).On<PreviewSubmit>().When(_ => throw new InvalidOperationException("boom"),
                "error guard").GoTo(PreviewState.Review);
            builder.State(PreviewState.Review);
        });

        var preview = await definition.PreviewAsync(ActiveStateShape<PreviewState>.Single(PreviewState.Draft),
            new PreviewSubmit());

        preview.AssertDenied(TransitionDenialReason.GuardEvaluationFailed);
        Assert.Contains(preview.GuardDiagnostics, guard => guard.Status == TransitionPreviewGuardStatus.Error);
    }
}
