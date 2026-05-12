using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class OpenTelemetryPublicApiSnapshotTests
{
    [Fact]
    public void OpenTelemetryPublicApiMatchesApprovedSnapshot()
    {
        PublicApiSnapshotAssert.MatchesApproved(PackableProject.All.Single(p => p.Name == "OpenTelemetry"));
    }
}