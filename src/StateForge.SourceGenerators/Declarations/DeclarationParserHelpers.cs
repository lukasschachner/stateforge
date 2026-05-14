using Microsoft.CodeAnalysis;
using StateForge.SourceGenerators.Emission;

namespace StateForge.SourceGenerators.Declarations;

internal static class DeclarationParserHelpers
{
    public static DeclaredState EnsureState(MachineDeclaration declaration, string expression, string key,
        Location? location)
    {
        var state = declaration.States.FirstOrDefault(s => s.IdentityKey == key);
        if (state is not null) return state;

        state = new DeclaredState(expression, expression, key,
            GeneratedNameHelper.IdentifierFromExpression(expression, "State_"), false, location);
        declaration.States.Add(state);
        return state;
    }
}
