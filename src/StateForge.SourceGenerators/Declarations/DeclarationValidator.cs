using Microsoft.CodeAnalysis;
using StateForge.SourceGenerators.Diagnostics;
using StateForge.SourceGenerators.Emission;

namespace StateForge.SourceGenerators.Declarations;

public static class DeclarationValidator
{
    public static void Validate(MachineDeclaration declaration, DiagnosticReporter reporter)
    {
        CheckDuplicates(declaration.States, s => s.IdentityKey, s => s.SourceLocation, "state", reporter);
        CheckDuplicates(declaration.Events, e => e.IdentityKey, e => e.SourceLocation, "event", reporter);
        GeneratedHelperAnalyzer.Analyze(declaration);
        CheckGeneratedMemberConflicts(declaration, reporter);
        TransitionDeclarationValidator.Validate(declaration, reporter);
        TerminalStateDeclarationValidator.Validate(declaration, reporter);
        AdvancedDeclarationValidator.Validate(declaration, reporter);
        MemberReferenceResolver.Validate(declaration, reporter);
        declaration.StaticGraph = StaticDeclarationGraphBuilder.Build(declaration);
        if (CanRunFlatStaticGraphAnalysis(declaration))
            StaticDeclarationGraphAnalyzer.Analyze(declaration.StaticGraph, reporter);
        declaration.GeneratedMetadata = GeneratedMetadataBuilder.Build(declaration);
    }

    private static bool CanRunFlatStaticGraphAnalysis(MachineDeclaration declaration)
    {
        return declaration.Composites.Count == 0 &&
               declaration.Regions.Count == 0 &&
               declaration.RegionMemberships.Count == 0 &&
               declaration.ParallelComposites.Count == 0;
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
            if (GeneratedMemberCatalog.RequiredMemberNames.Contains(member.Name))
                reporter.NameConflict(member.Name, member.Locations.FirstOrDefault() ?? declaration.SourceLocation);
    }
}