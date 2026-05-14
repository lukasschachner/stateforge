using StateForge.Core.Diagnostics;
using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Execution;

public sealed class TransitionDeniedGuardDiagnosticsTests
{
    [Fact]
    public async Task ActualGuardDeniedOutcomeReportsStructuredGuardDiagnostics()
    {
        var outcome = await TransitionPreviewTestDomain.Guarded(allow: false).ApplyAsync(PreviewState.Review,
            new PreviewApprove());

        Assert.Equal(TransitionOutcomeCategory.Denied, outcome.Category);
        var diagnostic = Assert.Single(outcome.DenialDiagnostics);
        Assert.Equal(TransitionDenialReason.FailedGuards, diagnostic.Reason);
        Assert.Contains(diagnostic.GuardDiagnostics,
            guard => guard.DisplayName == "approval guard" && guard.Status == TransitionPreviewGuardStatus.Failed);
    }
}
