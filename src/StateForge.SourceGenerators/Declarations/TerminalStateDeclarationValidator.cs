using StateForge.SourceGenerators.Diagnostics;

namespace StateForge.SourceGenerators.Declarations;

public static class TerminalStateDeclarationValidator
{
    public static void Validate(MachineDeclaration declaration, DiagnosticReporter reporter)
    {
        var terminal = declaration.States.Where(s => s.IsTerminal)
            .ToDictionary(s => s.IdentityKey, StringComparer.Ordinal);
        foreach (var transition in declaration.Transitions)
            if (terminal.TryGetValue(transition.SourceStateKey, out var state))
                reporter.InvalidTerminal(state.ValueExpression, transition.SourceLocation);
    }
}