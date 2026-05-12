using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class CoreFluentSampleTests
{
    [Fact]
    public void CoreFluentSampleRuns()
    {
        Assert.Contains("Core sample completed",
            CommandRunner.Run("dotnet",
                "run --project samples/Core.FluentSample/Core.FluentSample.csproj --configuration Release --no-build"));
    }
}