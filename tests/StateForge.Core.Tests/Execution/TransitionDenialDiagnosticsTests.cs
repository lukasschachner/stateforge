using StateForge.Core.Diagnostics;
using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Execution;

public sealed class TransitionDenialDiagnosticsTests
{
    [Fact]
    public async Task ActualDeniedOutcomeReportsNoMatchingEventDiagnostic()
    {
        var outcome = await TransitionPreviewTestDomain.Guarded().ApplyAsync(PreviewState.Draft,
            new PreviewReject());

        Assert.Equal(TransitionOutcomeCategory.NotPermitted, outcome.Category);
        Assert.Contains(outcome.DenialDiagnostics,
            diagnostic => diagnostic.Reason == TransitionDenialReason.UnknownEvent ||
                          diagnostic.Reason == TransitionDenialReason.NoMatchingEvent);
    }

    [Fact]
    public async Task PreviewReportsUnknownEventDiagnostic()
    {
        var preview = await TransitionPreviewTestDomain.Guarded().PreviewAsync(
            ActiveStateShape<PreviewState>.Single(PreviewState.Draft), new UnknownPreviewEvent());

        preview.AssertDenied(TransitionDenialReason.UnknownEvent);
    }

    [Fact]
    public async Task PreviewReportsTerminalStateDiagnostic()
    {
        var preview = await TransitionPreviewTestDomain.Guarded().PreviewAsync(
            ActiveStateShape<PreviewState>.Single(PreviewState.PreviewApproved), new PreviewSubmit());

        preview.AssertDenied(TransitionDenialReason.TerminalState);
    }
}
