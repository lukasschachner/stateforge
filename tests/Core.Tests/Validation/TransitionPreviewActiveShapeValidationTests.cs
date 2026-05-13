using Core.Tests.Execution;
using StateMachineLibrary.Core.Diagnostics;
using StateMachineLibrary.Core.Execution;

namespace Core.Tests.Validation;

public sealed class TransitionPreviewActiveShapeValidationTests
{
    [Fact]
    public async Task PreviewReportsInvalidActiveShapeForUnknownState()
    {
        var preview = await TransitionPreviewTestDomain.Guarded().PreviewAsync(
            ActiveStateShape<PreviewState>.Single((PreviewState)999), new PreviewSubmit());

        Assert.Equal(TransitionPreviewStatus.InvalidActiveShape, preview.Status);
        Assert.NotNull(preview.DenialDiagnostic);
        Assert.Equal(TransitionDenialReason.InvalidActiveShape, preview.DenialDiagnostic!.Reason);
    }
}
