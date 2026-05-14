using StateForge.Core;
using StateForge.DependencyInjection;
using StateForge.Logging;
using StateForge.OpenTelemetry;
using StateForge.Persistence;
using StateForge.Persistence.EntityFrameworkCore;
using StateForge.SourceGenerators;
using StateForge.Visualization.Graphviz;
using StateForge.Visualization.Mermaid;
using StateForge.Visualization.PlantUML;

namespace Release.Tests.TestSupport;

public sealed record PackableProject(
    string Name,
    string ProjectPath,
    string PackageId,
    string AssemblyName,
    string TargetFramework,
    Type MarkerType,
    string Responsibility,
    bool AnalyzerPackage = false)
{
    public string ApprovedSnapshotPath => $"tests/Release.Tests/PublicApi/{Name}.approved.txt";

    public static IReadOnlyList<PackableProject> All { get; } = new[]
    {
        new PackableProject(
            "Core",
            "src/StateForge.Core/StateForge.Core.csproj",
            "StateForge.Core",
            "Core",
            "net10.0",
            typeof(CorePublicApi),
            "dependency-light finite state machine definitions, runtime execution, validation, outcomes, introspection, and graph data"),
        new PackableProject(
            "DependencyInjection",
            "src/StateForge.DependencyInjection/StateForge.DependencyInjection.csproj",
            "StateForge.DependencyInjection",
            "DependencyInjection",
            "net10.0",
            typeof(DependencyInjectionPublicApi),
            "optional application composition helpers for state machine definitions and runtime factories"),
        new PackableProject(
            "Logging",
            "src/StateForge.Logging/StateForge.Logging.csproj",
            "StateForge.Logging",
            "Logging",
            "net10.0",
            typeof(LoggingPublicApi),
            "optional structured logging adapter for Core observations and validation diagnostics"),
        new PackableProject(
            "SourceGenerators",
            "src/StateForge.SourceGenerators/StateForge.SourceGenerators.csproj",
            "StateForge.SourceGenerators",
            "SourceGenerators",
            "netstandard2.0",
            typeof(SourceGeneratorsPublicApi),
            "optional analyzer/source-generator package for declaration-driven definitions",
            true),
        new PackableProject(
            "Persistence",
            "src/StateForge.Persistence/StateForge.Persistence.csproj",
            "StateForge.Persistence",
            "Persistence",
            "net10.0",
            typeof(PersistencePublicApi),
            "provider-neutral persistence contracts and coordination built on Core"),
        new PackableProject(
            "Persistence.EntityFrameworkCore",
            "src/StateForge.Persistence.EntityFrameworkCore/StateForge.Persistence.EntityFrameworkCore.csproj",
            "StateForge.Persistence.EntityFrameworkCore",
            "Persistence.EntityFrameworkCore",
            "net10.0",
            typeof(EntityFrameworkCorePublicApi),
            "optional provider-neutral EF Core snapshot storage adapter built on Persistence contracts"),
        new PackableProject(
            "OpenTelemetry",
            "src/StateForge.OpenTelemetry/StateForge.OpenTelemetry.csproj",
            "StateForge.OpenTelemetry",
            "OpenTelemetry",
            "net10.0",
            typeof(OpenTelemetryPublicApi),
            "optional OpenTelemetry-compatible activity and metric instrumentation built on Core"),
        new PackableProject(
            "Visualization.Mermaid",
            "src/StateForge.Visualization.Mermaid/StateForge.Visualization.Mermaid.csproj",
            "StateForge.Visualization.Mermaid",
            "Visualization.Mermaid",
            "net10.0",
            typeof(MermaidPublicApi),
            "optional deterministic Mermaid text renderer built on Core graph exports"),
        new PackableProject(
            "Visualization.Graphviz",
            "src/StateForge.Visualization.Graphviz/StateForge.Visualization.Graphviz.csproj",
            "StateForge.Visualization.Graphviz",
            "Visualization.Graphviz",
            "net10.0",
            typeof(GraphvizPublicApi),
            "optional deterministic Graphviz DOT text renderer built on Core graph exports"),
        new PackableProject(
            "Visualization.PlantUML",
            "src/StateForge.Visualization.PlantUML/StateForge.Visualization.PlantUML.csproj",
            "StateForge.Visualization.PlantUML",
            "Visualization.PlantUML",
            "net10.0",
            typeof(PlantUmlPublicApi),
            "optional deterministic PlantUML text renderer built on Core graph exports")
    };
}