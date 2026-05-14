namespace StateForge.SourceGenerators.Declarations;

public static class GeneratedGraphMetadataBuilder
{
    public static StaticDeclarationGraph Build(MachineDeclaration declaration)
    {
        return declaration.StaticGraph ?? StaticDeclarationGraphBuilder.Build(declaration);
    }
}
