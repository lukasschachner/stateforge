using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class CoreActionsSampleTests
{
    [Fact]
    public void CoreActionsSampleRuns()
    {
        Assert.Contains("Outcome: Success",
            CommandRunner.Run("dotnet",
                "run --project samples/Core.ActionsSample/Core.ActionsSample.csproj --configuration Release"));
    }
}