using Release.Tests.TestSupport;

namespace Release.Tests.Samples;

public sealed class PersistenceEntityFrameworkCoreSampleTests
{
    [Fact]
    public void PersistenceEntityFrameworkCoreSampleProjectExists()
    {
        Assert.True(File.Exists(ProjectPaths.FullPath(
            "samples/Persistence.EntityFrameworkCore.Sample/Persistence.EntityFrameworkCore.Sample.csproj")));
        Assert.True(File.Exists(ProjectPaths.FullPath("samples/Persistence.EntityFrameworkCore.Sample/Program.cs")));
    }
}
