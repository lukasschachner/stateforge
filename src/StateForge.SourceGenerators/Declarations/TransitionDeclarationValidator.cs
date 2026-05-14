using StateForge.SourceGenerators.Diagnostics;

namespace StateForge.SourceGenerators.Declarations;

public static class TransitionDeclarationValidator
{
    public static void Validate(MachineDeclaration declaration, DiagnosticReporter reporter)
    {
        var stateKeys = new HashSet<string>(declaration.States.Select(s => s.IdentityKey), StringComparer.Ordinal);
        var eventKeys = new HashSet<string>(declaration.Events.Select(e => e.IdentityKey), StringComparer.Ordinal);
        foreach (var transition in declaration.Transitions)
        {
            if (!stateKeys.Contains(transition.SourceStateKey))
                reporter.Missing("state", transition.SourceStateKey, transition.SourceLocation);
            if (!stateKeys.Contains(transition.TargetStateKey))
                reporter.Missing("state", transition.TargetStateKey, transition.SourceLocation);
            if (!eventKeys.Contains(transition.EventKey))
                reporter.Missing("event", transition.EventKey, transition.SourceLocation);
        }

        foreach (var group in declaration.Transitions.GroupBy(t =>
                     t.SourceStateKey + "\u001f" + t.EventKey + "\u001f" + t.TargetStateKey + "\u001f" + t.TransitionKind,
                     StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            var first = group.First();
            reporter.Duplicate("transition", first.SourceStateKey + " -> " + first.TargetStateKey,
                first.SourceLocation, group.Skip(1).Select(t => t.SourceLocation));
        }

        foreach (var group in declaration.Transitions.Where(t => t.Conditions.Count == 0)
                     .GroupBy(t => t.SourceStateKey + "\u001f" + t.EventKey, StringComparer.Ordinal)
                     .Where(g => g.Count() > 1))
        {
            var first = group.First();
            reporter.Ambiguous(first.SourceStateKey, first.EventKey, first.SourceLocation,
                group.Skip(1).Select(t => t.SourceLocation));
        }
    }
}