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

    public void Report(DiagnosticDescriptor descriptor, Location? location, IEnumerable<Location?>? relatedLocations,
        params object[] args)
    {
        var related = relatedLocations?.Where(l => l is not null).Select(l => l!).ToArray();
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