using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class EntityFrameworkCorePublicApiSnapshotTests
{
    [Fact]
    public void EntityFrameworkCorePublicApiMatchesApprovedSnapshot()
    {
        PublicApiSnapshotAssert.MatchesApproved(PackableProject.All.Single(p => p.Name == "Persistence.EntityFrameworkCore"));
    }
}
