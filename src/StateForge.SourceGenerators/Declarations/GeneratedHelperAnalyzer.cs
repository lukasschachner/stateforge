using StateForge.SourceGenerators.Emission;

namespace StateForge.SourceGenerators.Declarations;

public static class GeneratedHelperAnalyzer
{
    public static void Analyze(MachineDeclaration declaration)
    {
        declaration.GeneratedHelpers.Clear();
        var usedNames = new HashSet<string>(GeneratedMemberCatalog.RequiredMemberNames, StringComparer.Ordinal);
        foreach (var member in declaration.ContainingType.GetMembers()) usedNames.Add(member.Name);

        foreach (var declaredEvent in DeclarationOrdering.Events(declaration.Events))
        {
            var helperName = GeneratedNameHelper.EventHelperName(declaredEvent);
            var availability = GeneratedHelperAvailability.Generated;
            var reason = GeneratedHelperSkippedReason.None;

            if (declaredEvent.EventKind != DeclaredEventKind.Value || declaredEvent.ValueExpression is null)
            {
                availability = GeneratedHelperAvailability.Skipped;
                reason = GeneratedHelperSkippedReason.UnsupportedEventShape;
            }
            else if (!usedNames.Add(helperName))
            {
                availability = GeneratedHelperAvailability.Skipped;
                reason = GeneratedHelperSkippedReason.GeneratedNameConflict;
            }

            declaration.GeneratedHelpers.Add(new GeneratedHelperModel(declaredEvent.IdentityKey, helperName, availability,
                reason));
        }
    }
}
