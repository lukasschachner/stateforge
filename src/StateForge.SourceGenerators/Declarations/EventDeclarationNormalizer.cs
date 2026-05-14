namespace StateForge.SourceGenerators.Declarations;

public static class EventDeclarationNormalizer
{
    public static void EnsureTransitionEvents(MachineDeclaration declaration)
    {
        // Attribute declarations must explicitly declare events so missing references can be diagnosed.
        // DSL declarations add events as part of parsing supported On(...) calls.
    }
}