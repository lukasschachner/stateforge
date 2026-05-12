using Microsoft.CodeAnalysis;
using StateMachineLibrary.SourceGenerators.Diagnostics;

namespace StateMachineLibrary.SourceGenerators.Declarations;

public static class DeclarationValidator
{
    public static void Validate(MachineDeclaration declaration, DiagnosticReporter reporter)
    {
        CheckDuplicates(declaration.States, s => s.IdentityKey, s => s.SourceLocation, "state", reporter);
        CheckDuplicates(declaration.Events, e => e.IdentityKey, e => e.SourceLocation, "event", reporter);
        CheckGeneratedMemberConflicts(declaration, reporter);
        TransitionDeclarationValidator.Validate(declaration, reporter);
        TerminalStateDeclarationValidator.Validate(declaration, reporter);
        AdvancedDeclarationValidator.Validate(declaration, reporter);
        MemberReferenceResolver.Validate(declaration, reporter);
    }

    private static void CheckDuplicates<T>(IEnumerable<T> items, Func<T, string> key, Func<T, Location?> location,
        string kind, DiagnosticReporter reporter)
    {
        foreach (var group in items.GroupBy(key).Where(g => g.Count() > 1))
        {
            var first = group.First();
            reporter.Duplicate(kind, group.Key, location(first), group.Skip(1).Select(location));
        }
    }

    private static void CheckGeneratedMemberConflicts(MachineDeclaration declaration, DiagnosticReporter reporter)
    {
        foreach (var member in declaration.ContainingType.GetMembers())
            if (member.Name == "Definition" || member.Name == "CreateDefinition")
                reporter.NameConflict(member.Name, member.Locations.FirstOrDefault() ?? declaration.SourceLocation);
    }
}