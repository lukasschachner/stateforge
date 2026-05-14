using StateForge.SourceGenerators.Declarations;

namespace StateForge.SourceGenerators.Emission;

public static class MetadataEmitter
{
    public static string Emit(IEnumerable<MetadataEntry> metadata)
    {
        return string.Concat(metadata.Select(m =>
            ".WithMetadata(" + ConditionReferenceEmitter.Literal(m.Key) + ", " + m.ValueExpression + ")"));
    }
}