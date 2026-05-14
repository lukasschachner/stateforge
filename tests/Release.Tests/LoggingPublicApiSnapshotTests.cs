using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class LoggingPublicApiSnapshotTests
{
    [Fact]
    public void LoggingPublicApiMatchesApprovedSnapshot()
    {
        PublicApiSnapshotAssert.MatchesApproved(PackableProject.All.Single(p => p.Name == "Logging"));
    }
}
