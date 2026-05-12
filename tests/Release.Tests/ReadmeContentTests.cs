using Release.Tests.TestSupport;

namespace Release.Tests;

public sealed class ReadmeContentTests
{
    [Fact]
    public void ReadmeCoversUserJourneyAndBoundaries()
    {
        var readme = ProjectPaths.ReadAllText("README.md");
        foreach (var phrase in new[]
                 {
                     "What the library is", "What the library is not", "Package selection and installation",
                     "First fluent finite state machine", "Source-generator example", "Graph export",
                     "Provider-neutral persistence", "transition observation", "OpenTelemetry instrumentation",
                     "Release validation"
                 }) Assert.Contains(phrase, readme, StringComparison.OrdinalIgnoreCase);
        foreach (var packageId in PackableProject.All.Select(p => p.PackageId)) Assert.Contains(packageId, readme);
        foreach (var outOfScope in new[]
                 {
                     "workflow orchestration", "event sourcing", "hierarchical states", "parallel states",
                     "built-in database persistence providers", "visualization rendering", "exporter setup",
                     "dependency injection registration"
                 }) Assert.Contains(outOfScope, readme, StringComparison.OrdinalIgnoreCase);
    }
}