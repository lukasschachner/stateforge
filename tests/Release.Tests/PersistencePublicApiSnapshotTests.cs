using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class PersistencePublicApiSnapshotTests
{
    [Fact]
    public void PersistencePublicApiMatchesApprovedSnapshot()
    {
        PublicApiSnapshotAssert.MatchesApproved(PackableProject.All.Single(p => p.Name == "Persistence"));
    }
}