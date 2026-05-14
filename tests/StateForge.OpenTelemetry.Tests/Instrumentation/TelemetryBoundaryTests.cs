using StateForge.OpenTelemetry;

namespace StateForge.OpenTelemetry.Tests.Instrumentation;

public class TelemetryBoundaryTests
{
    [Fact]
    public void AdapterDoesNotReferenceExporterHostingOrDependencyInjectionAssemblies()
    {
        var references = typeof(StateMachineTelemetryObserver<,>).Assembly.GetReferencedAssemblies().Select(a => a.Name)
            .ToArray();

        Assert.DoesNotContain(references,
            name => name is not null && name.Contains("OpenTelemetry.Exporter", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(references,
            name => name is not null && name.Contains("Hosting", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(references,
            name => name is not null && name.Contains("DependencyInjection", StringComparison.OrdinalIgnoreCase));
    }
}