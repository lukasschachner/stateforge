using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class PersistenceSampleTests
{
    [Fact]
    public void PersistenceSampleRuns()
    {
        Assert.Contains("Persistence sample completed",
            CommandRunner.Run("dotnet",
                "run --project samples/Persistence.Sample/Persistence.Sample.csproj --configuration Release --no-build"));
    }
}