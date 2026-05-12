namespace StateMachineLibrary.SourceGenerators.Declarations;

public static class DeclarationNormalizer
{
    public static MachineDeclaration Normalize(MachineDeclaration declaration)
    {
        EventDeclarationNormalizer.EnsureTransitionEvents(declaration);
        return declaration;
    }
}