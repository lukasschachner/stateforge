using Microsoft.CodeAnalysis;

namespace StateForge.SourceGenerators.Declarations;

public sealed class MachineDeclaration
{
    public MachineDeclaration(DeclarationIdentity declarationId, INamedTypeSymbol containingType, string stateTypeName,
        string eventTypeName, DeclarationStyle declarationStyle, Location? sourceLocation = null)
    {
        DeclarationId = declarationId;
        ContainingType = containingType;
        StateTypeName = stateTypeName;
        EventTypeName = eventTypeName;
        DeclarationStyle = declarationStyle;
        SourceLocation = sourceLocation;
    }

    public DeclarationIdentity DeclarationId { get; }
    public INamedTypeSymbol ContainingType { get; }
    public string StateTypeName { get; }
    public string EventTypeName { get; }
    public DeclarationStyle DeclarationStyle { get; }
    public List<DeclaredState> States { get; } = new();
    public List<DeclaredEvent> Events { get; } = new();
    public List<DeclaredTransition> Transitions { get; } = new();
    public List<CompositeDeclaration> Composites { get; } = new();
    public List<ParallelCompositeDeclaration> ParallelComposites { get; } = new();
    public List<RegionDeclaration> Regions { get; } = new();
    public List<RegionMembership> RegionMemberships { get; } = new();
    public List<CompletionDeclaration> CompletionDeclarations { get; } = new();
    public List<GeneratedHelperModel> GeneratedHelpers { get; } = new();
    public GeneratedMetadataModel? GeneratedMetadata { get; set; }
    public StaticDeclarationGraph? StaticGraph { get; set; }
    public List<MetadataEntry> Metadata { get; } = new();
    public Location? SourceLocation { get; }
}