using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class SourceGeneratorSampleTests
{
    [Fact]
    public void SourceGeneratorSampleRuns()
    {
        Assert.Contains("Source generator sample completed",
            CommandRunner.Run("dotnet",
                "run --project samples/SourceGenerators.Sample/SourceGenerators.Sample.csproj --configuration Release --no-build"));
    }
}