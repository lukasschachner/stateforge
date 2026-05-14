using Microsoft.CodeAnalysis;

namespace StateForge.SourceGenerators.Declarations;

public enum DeclarationStyle
{
    Attribute,
    CompactDsl
}

public sealed class DeclarationLocation
{
    public DeclarationLocation(Location? primary, IReadOnlyList<Location>? related = null)
    {
        Primary = primary;
        Related = related ?? Array.Empty<Location>();
    }

    public Location? Primary { get; }
    public IReadOnlyList<Location> Related { get; }
}

public sealed class DeclarationIdentity
{
    public DeclarationIdentity(string containingTypeMetadataName, string machineName)
    {
        ContainingTypeMetadataName = containingTypeMetadataName;
        MachineName = machineName;
    }

    public string ContainingTypeMetadataName { get; }
    public string MachineName { get; }
    public string StableId => ContainingTypeMetadataName + ":" + MachineName;
}