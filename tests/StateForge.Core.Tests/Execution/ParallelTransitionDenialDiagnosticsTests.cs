using StateForge.Core.Diagnostics;

namespace StateForge.Core.Tests.Execution;

public sealed class ParallelTransitionDenialDiagnosticsTests
{
    [Fact]
    public async Task ParallelRuntimePreviewReportsNoMatchingEventDiagnostic()
    {
        var runtime = TransitionPreviewTestDomain.Parallel().CreateRuntime(PreviewState.Parallel);

        var preview = await runtime.PreviewAsync(new PreviewReject());

        preview.AssertDenied(TransitionDenialReason.UnknownEvent);
    }
}
