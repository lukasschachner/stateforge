using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class DependencyInjectionPublicApiSnapshotTests
{
    [Fact]
    public void DependencyInjectionPublicApiMatchesApprovedSnapshot()
    {
        PublicApiSnapshotAssert.MatchesApproved(PackableProject.All.Single(p => p.Name == "DependencyInjection"));
    }
}
