using StateForge.Core.Tests.Execution;
using StateForge.Core.Execution;

namespace StateForge.Core.Tests.Observation;

public sealed class TransitionPreviewObservationTests
{
    [Fact]
    public async Task RuntimePreviewDoesNotInvokeTransitionObservers()
    {
        var observer = new RecordingTransitionObserver<PreviewState, PreviewEvent>();
        var runtime = TransitionPreviewTestDomain.Guarded().CreateRuntime(PreviewState.Draft,
            observer: observer);

        var preview = await runtime.PreviewAsync(new PreviewSubmit());

        Assert.True(preview.IsPermitted);
        Assert.Empty(observer.Observations);
    }
}
