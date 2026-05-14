using Release.Tests.TestSupport;

namespace Release.Tests.Samples;

public sealed class ApplicationIntegrationSampleTests
{
    [Fact]
    public void ApplicationIntegrationSampleProjectExists()
    {
        Assert.True(File.Exists(ProjectPaths.FullPath("samples/ApplicationIntegration.Sample/ApplicationIntegration.Sample.csproj")));
        Assert.True(File.Exists(ProjectPaths.FullPath("samples/ApplicationIntegration.Sample/Program.cs")));
    }
}
