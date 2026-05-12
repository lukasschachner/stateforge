using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class SourceGeneratorsPublicApiSnapshotTests
{
    [Fact]
    public void SourceGeneratorsPublicApiMatchesApprovedSnapshot()
    {
        PublicApiSnapshotAssert.MatchesApproved(PackableProject.All.Single(p => p.Name == "SourceGenerators"));
    }
}