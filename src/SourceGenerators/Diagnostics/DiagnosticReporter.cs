using Microsoft.CodeAnalysis;

namespace StateMachineLibrary.SourceGenerators.Diagnostics;

public sealed class DiagnosticReporter
{
    private readonly List<Diagnostic> _diagnostics = new();

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;
    public bool HasErrors => _diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    public void Duplicate(string elementKind, string identity, Location? location,
        IEnumerable<Location?>? relatedLocations = null)
    {
        Report(StateMachineGeneratorDiagnostics.DuplicateDeclaration, location, relatedLocations, elementKind,
            identity);
    }

    public void Missing(string elementKind, string identity, Location? location)
    {
        Report(StateMachineGeneratorDiagnostics.MissingReference, location, null, elementKind, identity);
    }

    public void Ambiguous(string state, string eventIdentity, Location? location,
        IEnumerable<Location?>? relatedLocations = null)
    {
        Report(StateMachineGeneratorDiagnostics.AmbiguousTransition, location, relatedLocations, state, eventIdentity);
    }

    public void InvalidTerminal(string state, Location? location)
    {
        Report(StateMachineGeneratorDiagnostics.InvalidTerminalConfiguration, location, null, state);
    }

    public void UnsupportedDsl(string syntax, Location? location)
    {
        Report(StateMachineGeneratorDiagnostics.UnsupportedDslSyntax, location, null, syntax);
    }

    public void InvalidMember(string memberName, string usage, Location? location)
    {
        Report(StateMachineGeneratorDiagnostics.InvalidMemberReference, location, null, memberName, usage);
    }

    public void NameConflict(string memberName, Location? location)
    {
        Report(StateMachineGeneratorDiagnostics.GeneratedNameConflict, location, null, memberName);
    }

    public void DuplicateRegion(string owner, string region, Location? location,
        IEnumerable<Location?>? relatedLocations = null)
    {
        Report(StateMachineGeneratorDiagnostics.DuplicateRegionDeclaration, location, relatedLocations, owner, region);
    }

    public void MissingRegionalInitial(string owner, string region, Location? location)
    {
        Report(StateMachineGeneratorDiagnostics.MissingRegionalInitial, location, null, owner, region);
    }

    public void DuplicateRegionMembership(string state, string owner, Location? location,
        IEnumerable<Location?>? relatedLocations = null)
    {
        Report(StateMachineGeneratorDiagnostics.DuplicateRegionMembership, location, relatedLocations, state, owner);
    }

    public void UnknownRegionOwner(string owner, Location? location)
    {
        Report(StateMachineGeneratorDiagnostics.UnknownRegionOwner, location, null, owner);
    }

    public void UnsupportedHistory(string state, Location? location)
    {
        Report(StateMachineGeneratorDiagnostics.UnsupportedHistoryMode, location, null, state);
    }

    public void InvalidRoleCombination(string state, string reason, Location? location)
    {
        Report(StateMachineGeneratorDiagnostics.InvalidAdvancedRoleCombination, location, null, state, reason);
    }

    public void InvalidRegion(string owner, string region, string reason, Location? location)
    {
        Report(StateMachineGeneratorDiagnostics.InvalidRegionDeclaration, location, null, owner, region, reason);
    }

    public void UnreachableState(string state, Location? location)
    {
        Report(StateMachineGeneratorDiagnostics.UnreachableState, location, null, state);
    }

    public void DeadEndState(string state, Location? location)
    {
        Report(StateMachineGeneratorDiagnostics.DeadEndState, location, null, state);
    }

    public void TerminalNotReachable(string states, Location? location)
    {
        Report(StateMachineGeneratorDiagnostics.TerminalNotReachable, location, null, states);
    }

    public void Report(DiagnosticDescriptor descriptor, Location? location, IEnumerable<Location?>? relatedLocations,
        params object[] args)
    {
        var related = relatedLocations?.Where(l => l is not null)
            .Select(l => l!)
            .OrderBy(l => l.GetLineSpan().Path, StringComparer.Ordinal)
            .ThenBy(l => l.GetLineSpan().StartLinePosition.Line)
            .ThenBy(l => l.GetLineSpan().StartLinePosition.Character)
            .ToArray();
        _diagnostics.Add(Diagnostic.Create(descriptor, location, related, args));
    }

    public void ReportTo(SourceProductionContext context)
    {
        foreach (var diagnostic in _diagnostics.OrderBy(d => d.Id)
                     .ThenBy(d => d.Location?.GetLineSpan().Path, StringComparer.Ordinal)
                     .ThenBy(d => d.Location?.GetLineSpan().StartLinePosition.Line ?? 0)
                     .ThenBy(d => d.Location?.GetLineSpan().StartLinePosition.Character ?? 0))
            context.ReportDiagnostic(diagnostic);
    }
}