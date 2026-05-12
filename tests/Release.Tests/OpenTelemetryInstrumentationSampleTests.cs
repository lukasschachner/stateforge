using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class OpenTelemetryInstrumentationSampleTests
{
    [Fact]
    public void OpenTelemetryInstrumentationSampleRuns()
    {
        Assert.Contains("OpenTelemetry instrumentation sample completed",
            CommandRunner.Run("dotnet",
                "run --project samples/OpenTelemetry.InstrumentationSample/OpenTelemetry.InstrumentationSample.csproj --configuration Release --no-build"));
    }
}