using StateForge.Core.Diagnostics;
using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Execution;

public sealed class TransitionPreviewDefinitionTests
{
    [Fact]
    public async Task PreviewAsyncReturnsPermittedSelectedTransitionAndExpectedShape()
    {
        var definition = TransitionPreviewTestDomain.Guarded();

        var preview = await definition.PreviewAsync(
            ActiveStateShape<PreviewState>.Single(PreviewState.Draft), new PreviewSubmit());

        preview.AssertPermitted(PreviewState.Review);
        Assert.Equal(PreviewState.Review, preview.ExpectedActiveShape?.ActiveLeafState);
        Assert.NotEmpty(preview.CandidateTransitions);
    }

    [Fact]
    public async Task PreviewAsyncReportsFailedGuardWithoutCommit()
    {
        var definition = TransitionPreviewTestDomain.Guarded(allow: false);

        var preview = await definition.PreviewAsync(
            ActiveStateShape<PreviewState>.Single(PreviewState.Review), new PreviewApprove());

        preview.AssertDenied(TransitionDenialReason.FailedGuards);
        Assert.Contains(preview.GuardDiagnostics, guard =>
            guard.DisplayName == "approval guard" && guard.Status == TransitionPreviewGuardStatus.Failed);
    }
}
