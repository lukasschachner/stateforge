using StateForge.SourceGenerators.Declarations;

namespace StateForge.SourceGenerators.Emission;

public sealed class GeneratedMachineDefinition
{
    public GeneratedMachineDefinition(MachineDeclaration declaration, string generatedSourceName, string source)
    {
        Declaration = declaration;
        GeneratedSourceName = generatedSourceName;
        Source = source;
    }

    public MachineDeclaration Declaration { get; }
    public string GeneratedSourceName { get; }
    public string Source { get; }
}