using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class InteractiveApiFrontendSampleTests
{
    [Fact]
    public void InteractiveApiFrontendSampleSmokeTestRuns()
    {
        var output = CommandRunner.Run("dotnet",
            "run --project samples/Interactive.ApiFrontendSample/Interactive.ApiFrontendSample.csproj --configuration Release --no-build -- --smoke-test");

        Assert.Contains("[smoke] Preview CapturePayment from Draft", output, StringComparison.Ordinal);
        Assert.Contains("[smoke] StartPacking before full capture -> category=NotPermitted, committed=False", output,
            StringComparison.Ordinal);
        Assert.Contains("Interactive API frontend sample smoke test completed: state=Completed", output,
            StringComparison.Ordinal);
    }
}
