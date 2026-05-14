namespace StateForge.SourceGenerators.Emission;

public static class GeneratedMemberCatalog
{
    public const string Definition = nameof(Definition);
    public const string CreateDefinition = nameof(CreateDefinition);
    public const string GeneratedMetadata = nameof(GeneratedMetadata);
    public const string GeneratedGraph = nameof(GeneratedGraph);

    public static IReadOnlyCollection<string> RequiredMemberNames { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        Definition,
        CreateDefinition,
        GeneratedMetadata,
        GeneratedGraph
    };
}
