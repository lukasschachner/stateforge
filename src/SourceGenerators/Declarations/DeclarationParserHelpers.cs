using Microsoft.CodeAnalysis;
using StateMachineLibrary.SourceGenerators.Emission;

namespace StateMachineLibrary.SourceGenerators.Declarations;

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
