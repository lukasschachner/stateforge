using StateMachineLibrary.Core;
using StateMachineLibrary.OpenTelemetry;
using StateMachineLibrary.Persistence;
using StateMachineLibrary.SourceGenerators;
using StateMachineLibrary.Visualization.Graphviz;
using StateMachineLibrary.Visualization.Mermaid;
using StateMachineLibrary.Visualization.PlantUML;

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
            "src/Core/Core.csproj",
            "StateMachineLibrary.Core",
            "Core",
            "net10.0",
            typeof(CorePublicApi),
            "dependency-light finite state machine definitions, runtime execution, validation, outcomes, introspection, and graph data"),
        new PackableProject(
            "SourceGenerators",
            "src/SourceGenerators/SourceGenerators.csproj",
            "StateMachineLibrary.SourceGenerators",
            "SourceGenerators",
            "netstandard2.0",
            typeof(SourceGeneratorsPublicApi),
            "optional analyzer/source-generator package for declaration-driven definitions",
            true),
        new PackableProject(
            "Persistence",
            "src/Persistence/Persistence.csproj",
            "StateMachineLibrary.Persistence",
            "Persistence",
            "net10.0",
            typeof(PersistencePublicApi),
            "provider-neutral persistence contracts and coordination built on Core"),
        new PackableProject(
            "OpenTelemetry",
            "src/OpenTelemetry/OpenTelemetry.csproj",
            "StateMachineLibrary.OpenTelemetry",
            "OpenTelemetry",
            "net10.0",
            typeof(OpenTelemetryPublicApi),
            "optional OpenTelemetry-compatible activity and metric instrumentation built on Core"),
        new PackableProject(
            "Visualization.Mermaid",
            "src/Visualization.Mermaid/Visualization.Mermaid.csproj",
            "StateMachineLibrary.Visualization.Mermaid",
            "Visualization.Mermaid",
            "net10.0",
            typeof(MermaidPublicApi),
            "optional deterministic Mermaid text renderer built on Core graph exports"),
        new PackableProject(
            "Visualization.Graphviz",
            "src/Visualization.Graphviz/Visualization.Graphviz.csproj",
            "StateMachineLibrary.Visualization.Graphviz",
            "Visualization.Graphviz",
            "net10.0",
            typeof(GraphvizPublicApi),
            "optional deterministic Graphviz DOT text renderer built on Core graph exports"),
        new PackableProject(
            "Visualization.PlantUML",
            "src/Visualization.PlantUML/Visualization.PlantUML.csproj",
            "StateMachineLibrary.Visualization.PlantUML",
            "Visualization.PlantUML",
            "net10.0",
            typeof(PlantUmlPublicApi),
            "optional deterministic PlantUML text renderer built on Core graph exports")
    };
}