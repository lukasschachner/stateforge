using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Diagnostics;

internal static class TransitionConflictDiagnosticBuilder
{
    public static IReadOnlyList<TransitionConflictDiagnostic> BuildValidationDiagnostics<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        IReadOnlyList<ValidationFinding> findings)
    {
        var diagnostics = new List<TransitionConflictDiagnostic>();
        AddDuplicateSourceDiagnostics(definition, findings, diagnostics);
        AddCrossRegionDiagnostics(definition, findings, diagnostics);
        AddCompletionDiagnostics(definition, findings, diagnostics);
        AddInvalidTargetDiagnostics(findings, diagnostics);
        return diagnostics;
    }

    public static TransitionConflictDiagnostic ParentRegionalConflict<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        TransitionDefinition<TState, TEvent> parentTransition,
        IReadOnlyList<TransitionDefinition<TState, TEvent>> regionalTransitions,
        TEvent? @event = default)
    {
        var participants = new List<TransitionConflictParticipant>
        {
            CreateTransitionParticipant(definition, parentTransition, TransitionConflictParticipantRole.ParentTransition,
                0)
        };

        participants.AddRange(regionalTransitions.Select((transition, index) =>
            CreateTransitionParticipant(definition, transition, TransitionConflictParticipantRole.RegionalTransition,
                index + 1)));

        return new TransitionConflictDiagnostic(
            TransitionConflictKind.ParentRegionalConflict,
            "Parent-level transition conflicts with selected regional transitions for the same dispatch.",
            TransitionTriggerKind.Event,
            @event,
            parentTransition.Event.Identity,
            conflictScope: parentTransition.SourceState,
            compositeState: parentTransition.SourceState,
            participants: participants);
    }

    public static TransitionConflictDiagnostic DuplicateSourceConflict<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        IReadOnlyList<TransitionDefinition<TState, TEvent>> transitions,
        string? validationCode = null,
        object? @event = null)
    {
        var first = transitions[0];
        return new TransitionConflictDiagnostic(
            TransitionConflictKind.DuplicateSourceScope,
            $"Multiple transitions selected for source scope '{first.SourceState}'.",
            first.TriggerKind,
            @event,
            first.Event.Identity,
            conflictScope: first.SourceState,
            participants: transitions.Select((transition, index) =>
                CreateTransitionParticipant(definition, transition, TransitionConflictParticipantRole.CompetingTransition,
                    index)),
            validationCode: validationCode);
    }

    public static TransitionConflictDiagnostic CrossRegionBoundary<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        TransitionDefinition<TState, TEvent> transition,
        string? validationCode = null,
        object? @event = null)
    {
        definition.TryGetCommonParallelOwner(transition.SourceState, transition.TargetState, out var owner,
            out var sourceRegionId, out var targetRegionId);
        var sourceRegionName = GetRegionName(definition, sourceRegionId);
        var targetRegionName = GetRegionName(definition, targetRegionId);
        var participants = new List<TransitionConflictParticipant>
        {
            CreateTransitionParticipant(definition, transition, TransitionConflictParticipantRole.CompetingTransition, 0,
                sourceRegionId, sourceRegionName),
            new(TransitionConflictParticipantRole.Region, regionId: sourceRegionId, regionName: sourceRegionName,
                compositeState: owner, order: 1),
            new(TransitionConflictParticipantRole.Region, regionId: targetRegionId, regionName: targetRegionName,
                compositeState: owner, order: 2)
        };

        return new TransitionConflictDiagnostic(
            TransitionConflictKind.CrossRegionBoundary,
            $"Transition '{transition}' crosses parallel region boundaries.",
            transition.TriggerKind,
            @event,
            transition.Event.Identity,
            conflictScope: owner,
            compositeState: owner,
            sourceRegionId: sourceRegionId,
            sourceRegionName: sourceRegionName,
            targetRegionId: targetRegionId,
            targetRegionName: targetRegionName,
            participants: participants,
            validationCode: validationCode);
    }

    public static TransitionConflictDiagnostic CompletionConflict<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        TState completionScope,
        IReadOnlyList<CompletionTransitionDefinition<TState, TEvent>> transitions,
        string? message = null,
        string? validationCode = null)
    {
        return new TransitionConflictDiagnostic(
            TransitionConflictKind.CompletionConflict,
            message ?? $"Multiple completion transitions compete for scope '{completionScope}'.",
            TransitionTriggerKind.Completion,
            eventIdentity: "completion",
            completionScope: completionScope,
            conflictScope: completionScope,
            participants: transitions.Select((transition, index) =>
                CreateCompletionParticipant(definition, transition,
                    TransitionConflictParticipantRole.CompletionTransition, index)),
            validationCode: validationCode);
    }

    public static TransitionConflictDiagnostic InvalidPostShape(
        string message,
        object? conflictScope,
        string? validationCode = null,
        IEnumerable<TransitionConflictParticipant>? participants = null,
        InvalidActiveShapeDiagnostic? invalidShape = null)
    {
        return new TransitionConflictDiagnostic(
            TransitionConflictKind.InvalidPostShape,
            message,
            conflictScope: conflictScope,
            participants: participants,
            invalidShape: invalidShape,
            validationCode: validationCode);
    }

    public static TransitionConflictParticipant CreateTransitionParticipant<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        TransitionDefinition<TState, TEvent> transition,
        TransitionConflictParticipantRole role,
        int order,
        string? regionId = null,
        string? regionName = null,
        GuardOutcomeDiagnostic? guardOutcome = null)
    {
        if (regionId is null && definition.TryGetRegionMembership(transition.SourceState, out var membership))
        {
            regionId = membership.RegionId;
            regionName = GetRegionName(definition, regionId);
        }

        var transitionId = TransitionIdentityProvider.GetTransitionId(definition, transition);
        return new TransitionConflictParticipant(
            role,
            transitionId,
            transition.TriggerKind,
            eventIdentity: transition.Event.Identity,
            sourceState: transition.SourceState,
            targetState: transition.TargetState,
            regionId: regionId,
            regionName: regionName,
            guardOutcome: guardOutcome,
            order: order);
    }

    public static TransitionConflictParticipant CreateCompletionParticipant<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        CompletionTransitionDefinition<TState, TEvent> transition,
        TransitionConflictParticipantRole role,
        int order,
        GuardOutcomeDiagnostic? guardOutcome = null)
    {
        var transitionId = TransitionIdentityProvider.GetTransitionId(definition, transition);
        return new TransitionConflictParticipant(
            role,
            transitionId,
            TransitionTriggerKind.Completion,
            eventIdentity: "completion",
            sourceState: transition.SourceState,
            targetState: transition.TargetState,
            guardOutcome: guardOutcome,
            order: order);
    }

    private static void AddDuplicateSourceDiagnostics<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        IReadOnlyList<ValidationFinding> findings,
        ICollection<TransitionConflictDiagnostic> diagnostics)
    {
        var duplicateCode = definition.HasHierarchy
            ? HierarchyValidationCodes.AmbiguousTransition
            : "TRANSITION003";
        foreach (var group in definition.Transitions
                     .Select((transition, index) => (transition, index))
                     .GroupBy(x => (x.transition.SourceState, x.transition.Event.Identity))
                     .Where(group => group.Count() > 1)
                     .OrderBy(group => group.Min(x => x.index)))
        {
            var transitions = group.OrderBy(x => x.index).Select(x => x.transition).ToArray();
            var code = findings.Any(f => string.Equals(f.Code, ParallelValidationCodes.AmbiguousEvent,
                    StringComparison.Ordinal) && Equals(f.SourceState, group.Key.SourceState))
                ? ParallelValidationCodes.AmbiguousEvent
                : duplicateCode;
            diagnostics.Add(DuplicateSourceConflict(definition, transitions, code));
        }
    }

    private static void AddCrossRegionDiagnostics<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        IReadOnlyList<ValidationFinding> findings,
        ICollection<TransitionConflictDiagnostic> diagnostics)
    {
        foreach (var transition in definition.Transitions)
        {
            if (!definition.TryGetCommonParallelOwner(transition.SourceState, transition.TargetState, out _,
                    out var sourceRegionId, out var targetRegionId) ||
                StringComparer.Ordinal.Equals(sourceRegionId, targetRegionId))
                continue;

            var code = findings.FirstOrDefault(f => string.Equals(f.AffectedElement, transition.ToString(),
                StringComparison.Ordinal))?.Code;
            diagnostics.Add(CrossRegionBoundary(definition, transition, code));
        }

        foreach (var transition in definition.CompletionTransitions)
        {
            var executable = transition.ToExecutableTransition(false);
            if (!definition.TryGetCommonParallelOwner(executable.SourceState, executable.TargetState, out _,
                    out var sourceRegionId, out var targetRegionId) ||
                StringComparer.Ordinal.Equals(sourceRegionId, targetRegionId))
                continue;

            var code = findings.FirstOrDefault(f => string.Equals(f.AffectedElement, transition.ToString(),
                StringComparison.Ordinal))?.Code;
            diagnostics.Add(CrossRegionBoundary(definition, executable, code));
        }
    }

    private static void AddCompletionDiagnostics<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        IReadOnlyList<ValidationFinding> findings,
        ICollection<TransitionConflictDiagnostic> diagnostics)
    {
        foreach (var group in definition.CompletionTransitions
                     .Where(t => t.Conditions.Count == 0)
                     .GroupBy(t => t.SourceState)
                     .Where(group => group.Count() > 1)
                     .OrderBy(group => group.Min(t => t.DeclarationOrder)))
        {
            var code = findings.FirstOrDefault(f => string.Equals(f.AffectedElement, $"completion:{group.Key}",
                StringComparison.Ordinal))?.Code ?? CompletionTransitionValidationCodes.AmbiguousUnguarded;
            diagnostics.Add(CompletionConflict(definition, group.Key,
                group.OrderBy(t => t.DeclarationOrder).ToArray(), validationCode: code));
        }
    }

    private static void AddInvalidTargetDiagnostics(
        IReadOnlyList<ValidationFinding> findings,
        ICollection<TransitionConflictDiagnostic> diagnostics)
    {
        foreach (var finding in findings.Where(f => string.Equals(f.Code, "TRANSITION002", StringComparison.Ordinal) ||
                                                    string.Equals(f.Code,
                                                        CompletionTransitionValidationCodes.InvalidTarget,
                                                        StringComparison.Ordinal)))
        {
            diagnostics.Add(InvalidPostShape(
                finding.Message,
                finding.SourceState,
                finding.Code,
                [
                    new TransitionConflictParticipant(
                        TransitionConflictParticipantRole.CompetingTransition,
                        finding.TransitionId,
                        string.Equals(finding.Event?.ToString(), "completion", StringComparison.Ordinal)
                            ? TransitionTriggerKind.Completion
                            : TransitionTriggerKind.Event,
                        eventIdentity: finding.Event?.ToString(),
                        sourceState: finding.SourceState,
                        targetState: finding.TargetState)
                ],
                new InvalidActiveShapeDiagnostic(expectedShape: "Transition target must resolve to a declared active state.")));
        }
    }

    private static string? GetRegionName<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        string? regionId)
    {
        return regionId is not null && definition.TryGetParallelRegion(regionId, out var region) ? region.Name : null;
    }
}
