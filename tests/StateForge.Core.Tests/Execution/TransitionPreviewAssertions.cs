using StateForge.Core.Diagnostics;
using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Execution;

internal static class TransitionPreviewAssertions
{
    public static void AssertPermitted<TState, TEvent>(this TransitionPreviewResult<TState, TEvent> preview,
        TState expectedTarget)
    {
        Assert.True(preview.IsPermitted);
        Assert.Equal(TransitionPreviewStatus.Permitted, preview.Status);
        Assert.Equal(expectedTarget, preview.ExpectedTargetState);
        Assert.NotNull(preview.SelectedTransition);
        Assert.Null(preview.DenialDiagnostic);
    }

    public static void AssertDenied<TState, TEvent>(this TransitionPreviewResult<TState, TEvent> preview,
        TransitionDenialReason reason)
    {
        Assert.False(preview.IsPermitted);
        Assert.NotNull(preview.DenialDiagnostic);
        Assert.Equal(reason, preview.DenialDiagnostic!.Reason);
    }
}
