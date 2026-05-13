namespace StateMachineLibrary.SourceGenerators.Declarations;

public static class GeneratedMetadataBuilder
{
    private const string CompletionTransitionName = "completion";

    public static GeneratedMetadataModel Build(MachineDeclaration declaration)
    {
        var helpers = declaration.GeneratedHelpers.ToDictionary(h => h.EventKey, StringComparer.Ordinal);
        return new GeneratedMetadataModel(
            declaration.DeclarationId.StableId,
            DeclarationOrdering.States(declaration.States)
                .Select(s => new GeneratedStateMetadata(s.IdentityKey, s.Name, s.IsTerminal))
                .ToArray(),
            DeclarationOrdering.Events(declaration.Events)
                .Select(e =>
                {
                    helpers.TryGetValue(e.IdentityKey, out var helper);
                    return new GeneratedEventMetadata(e.IdentityKey, e.Name, e.EventKind,
                        helper?.Availability ?? GeneratedHelperAvailability.Skipped,
                        helper?.SkippedReason ?? GeneratedHelperSkippedReason.None);
                }).ToArray(),
            DeclarationOrdering.Transitions(declaration.Transitions)
                .Select(t => new GeneratedTransitionMetadata(t.TransitionId, t.SourceStateKey, t.EventKey,
                    t.TargetStateKey, t.TransitionKind))
                .Concat(declaration.CompletionDeclarations.OrderBy(c => c.SourceStateKey, StringComparer.Ordinal)
                    .Select(c => new GeneratedTransitionMetadata(CompletionTransitionName, c.SourceStateKey,
                        CompletionTransitionName, c.TargetStateKey, DeclaredTransitionKind.External)))
                .ToArray());
    }
}
