using StateMachineLibrary.SourceGenerators.Diagnostics;

namespace StateMachineLibrary.SourceGenerators.Declarations;

public static class AdvancedDeclarationValidator
{
    private const string KeySeparator = "\u001f";

    public static void Validate(MachineDeclaration declaration, DiagnosticReporter reporter)
    {
        ValidateHistory(declaration, reporter);
        ValidateRegions(declaration, reporter);
        ValidateMemberships(declaration, reporter);
        ValidateRoleCombinations(declaration, reporter);
    }

    private static void ValidateHistory(MachineDeclaration declaration, DiagnosticReporter reporter)
    {
        foreach (var state in declaration.States)
        {
            if (state.HistoryMode == DeclaredHistoryMode.Unsupported)
                reporter.UnsupportedHistory(state.Name, state.HistoryLocation ?? state.SourceLocation);

            if (state.HistoryMode != DeclaredHistoryMode.None && state.IsTerminal)
                reporter.InvalidRoleCombination(state.Name, "terminal states cannot also declare history",
                    state.HistoryLocation ?? state.SourceLocation);
        }
    }

    private static void ValidateRegions(MachineDeclaration declaration, DiagnosticReporter reporter)
    {
        var explicitRegions = declaration.Regions.Where(r => r.IsExplicit).GroupBy(r =>
            r.OwnerCompositeStateKey + KeySeparator + r.RegionName, StringComparer.Ordinal);
        foreach (var group in explicitRegions)
        {
            var regions = group.ToArray();
            if (regions.Length <= 1) continue;
            var first = regions[0];
            reporter.DuplicateRegion(first.OwnerCompositeExpression, first.RegionName, first.SourceLocation,
                regions.Skip(1).Select(r => r.SourceLocation));
        }

        foreach (var region in CanonicalRegions(declaration))
        {
            if (string.IsNullOrWhiteSpace(region.RegionName))
                reporter.InvalidRegion(region.OwnerCompositeExpression, region.RegionName, "region name must be non-empty",
                    region.SourceLocation);

            if (!declaration.ParallelComposites.Any(p => p.CompositeStateKey == region.OwnerCompositeStateKey))
                reporter.UnknownRegionOwner(region.OwnerCompositeExpression, region.SourceLocation);

            if (region.InitialStateKey is null)
                reporter.MissingRegionalInitial(region.OwnerCompositeExpression, region.RegionName,
                    region.SourceLocation);
        }
    }

    private static void ValidateMemberships(MachineDeclaration declaration, DiagnosticReporter reporter)
    {
        var memberships = declaration.RegionMemberships;
        foreach (var group in memberships.GroupBy(m => m.OwnerCompositeStateKey + KeySeparator + m.StateKey,
                     StringComparer.Ordinal))
        {
            var regions = group.Select(m => m.RegionName).Distinct(StringComparer.Ordinal).ToArray();
            if (regions.Length <= 1) continue;
            var first = group.First();
            reporter.DuplicateRegionMembership(first.StateExpression, first.OwnerCompositeExpression,
                first.SourceLocation, group.Skip(1).Select(m => m.SourceLocation));
        }

        foreach (var membership in memberships.Where(m => m.IsTerminal))
        {
            var matchingMember = memberships.Any(m => !m.IsTerminal &&
                m.OwnerCompositeStateKey == membership.OwnerCompositeStateKey &&
                m.RegionName == membership.RegionName &&
                m.StateKey == membership.StateKey);
            var matchingInitial = memberships.Any(m => m.IsInitial &&
                m.OwnerCompositeStateKey == membership.OwnerCompositeStateKey &&
                m.RegionName == membership.RegionName &&
                m.StateKey == membership.StateKey);
            if (!matchingMember && !matchingInitial)
            {
                // A terminal-only membership is accepted as shorthand and normalized into the region.
                continue;
            }
        }
    }

    private static void ValidateRoleCombinations(MachineDeclaration declaration, DiagnosticReporter reporter)
    {
        foreach (var state in declaration.States)
        {
            if (state.IsTerminal && state.IsParallelComposite)
                reporter.InvalidRoleCombination(state.Name, "terminal states cannot be parallel composites",
                    state.SourceLocation);

            if (state.IsTerminal && state.InitialChildStateKey is not null)
                reporter.InvalidRoleCombination(state.Name, "terminal states cannot declare an initial child",
                    state.InitialChildLocation ?? state.SourceLocation);
        }
    }

    private static IEnumerable<RegionDeclaration> CanonicalRegions(MachineDeclaration declaration)
    {
        return declaration.Regions
            .GroupBy(r => r.OwnerCompositeStateKey + KeySeparator + r.RegionName, StringComparer.Ordinal)
            .Select(g => g.OrderBy(r => r.Order).First());
    }
}
