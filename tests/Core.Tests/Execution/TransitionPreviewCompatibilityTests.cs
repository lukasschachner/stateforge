using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Execution;

public sealed class TransitionPreviewCompatibilityTests
{
    [Fact]
    public async Task ExistingApplyAsyncSuccessAndDeniedCategoriesRemainStable()
    {
        var definition = TransitionPreviewTestDomain.Guarded(allow: false);

        var success = await definition.ApplyAsync(PreviewState.Draft, new PreviewSubmit());
        var denied = await definition.ApplyAsync(PreviewState.Review, new PreviewApprove());
        var notPermitted = await definition.ApplyAsync(PreviewState.Draft, new PreviewReject());

        Assert.Equal(TransitionOutcomeCategory.Success, success.Category);
        Assert.Equal(TransitionOutcomeCategory.Denied, denied.Category);
        Assert.Equal(TransitionOutcomeCategory.NotPermitted, notPermitted.Category);
        Assert.True(success.Committed);
        Assert.False(denied.Committed);
        Assert.False(notPermitted.Committed);
    }
}
