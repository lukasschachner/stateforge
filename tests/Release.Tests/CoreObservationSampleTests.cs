using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class CoreObservationSampleTests
{
    [Fact]
    public void CoreObservationSampleRuns()
    {
        Assert.Contains("Core observation sample completed",
            CommandRunner.Run("dotnet",
                "run --project samples/Core.ObservationSample/Core.ObservationSample.csproj --configuration Release --no-build"));
    }
}