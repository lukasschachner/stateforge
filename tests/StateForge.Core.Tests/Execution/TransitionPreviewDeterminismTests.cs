namespace StateForge.Core.Tests.Execution;

public sealed class TransitionPreviewDeterminismTests
{
    [Fact]
    public async Task PreviewCandidateAndGuardDiagnosticsUseDeclarationOrder()
    {
        var definition = TransitionPreviewTestDomain.Guarded(allow: true);

        var first = await definition.PreviewAsync(TransitionPreviewTestDomain.Shape(PreviewState.Review),
            new PreviewApprove());
        var second = await definition.PreviewAsync(TransitionPreviewTestDomain.Shape(PreviewState.Review),
            new PreviewApprove());

        Assert.Equal(first.CandidateTransitions.Select(c => c.TransitionId),
            second.CandidateTransitions.Select(c => c.TransitionId));
        Assert.All(first.CandidateTransitions, candidate => Assert.StartsWith("transition-", candidate.TransitionId));
        Assert.All(first.GuardDiagnostics, diagnostic => Assert.StartsWith("transition-", diagnostic.TransitionId));
        Assert.Equal(first.GuardDiagnostics.Select(g => (g.GuardIndex, g.DisplayName, g.Status)),
            second.GuardDiagnostics.Select(g => (g.GuardIndex, g.DisplayName, g.Status)));
    }
}
