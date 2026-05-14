using StateForge.Core.Definitions;
using StateForge.Core.Diagnostics;
using StateForge.Core.Execution;

namespace StateForge.Core.Validation;

/// <summary>Validates reusable state machine definitions before execution.</summary>
public static class MachineDefinitionValidator
{
    public static ValidationResult Validate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition)
    {
        var findings = new List<ValidationFinding>();
        var comparer = EqualityComparer<TState>.Default;

        if (definition.States.Count == 0)
            findings.Add(new ValidationFinding(ValidationSeverity.Error, "STATE001",
                "At least one state must be declared.", "states", "Declare one or more states."));

        var duplicateStates = definition.States.GroupBy(s => s.Value, comparer).Where(g => g.Count() > 1);
        foreach (var group in duplicateStates)
            findings.Add(new ValidationFinding(ValidationSeverity.Error, "STATE002",
                $"State '{group.Key}' is declared more than once.", $"state:{group.Key}"));

        foreach (var state in definition.States)
            foreach (var action in state.ExitActions.Concat(state.EntryActions))
            {
                if (action.Kind is not (ActionKind.Entry or ActionKind.Exit))
                    findings.Add(new ValidationFinding(ValidationSeverity.Error, "ACTION001",
                        $"State action '{action.DisplayName}' has invalid kind '{action.Kind}'.",
                        $"state:{state.Value}:action:{action.Order}"));

                if ((action.Kind == ActionKind.Entry && action.Phase != TransitionLifecyclePhase.Entry) ||
                    (action.Kind == ActionKind.Exit && action.Phase != TransitionLifecyclePhase.Exit))
                    findings.Add(new ValidationFinding(ValidationSeverity.Error, "ACTION002",
                        $"State action '{action.DisplayName}' has invalid phase '{action.Phase}' for kind '{action.Kind}'.",
                        $"state:{state.Value}:action:{action.Order}"));

                if (string.IsNullOrWhiteSpace(action.DisplayName))
                    findings.Add(new ValidationFinding(ValidationSeverity.Error, "ACTION003",
                        "State action display name must be non-empty.", $"state:{state.Value}:action:{action.Order}"));
            }

        foreach (var transition in definition.Transitions)
        {
            if (!definition.ContainsState(transition.SourceState))
                findings.Add(new ValidationFinding(ValidationSeverity.Error, "TRANSITION001",
                    $"Transition source state '{transition.SourceState}' is not declared.", transition.ToString()));

            if (!definition.ContainsState(transition.TargetState))
                findings.Add(new ValidationFinding(ValidationSeverity.Error, "TRANSITION002",
                    $"Transition target state '{transition.TargetState}' is not declared.", transition.ToString(),
                    "Declare the target state or choose a declared target."));

            foreach (var action in transition.TransitionActions)
            {
                if (string.IsNullOrWhiteSpace(action.DisplayName))
                    findings.Add(new ValidationFinding(ValidationSeverity.Error, "ACTION004",
                        "Transition action display name must be non-empty.",
                        $"transition:{transition}:action:{action.Order}"));

                if (action.Phase != TransitionLifecyclePhase.Transition)
                    findings.Add(new ValidationFinding(ValidationSeverity.Error, "ACTION005",
                        $"Transition action '{action.DisplayName}' has invalid phase '{action.Phase}'.",
                        $"transition:{transition}:action:{action.Order}"));
            }

            var source = definition.FindState(transition.SourceState);
            if (source?.IsTerminal == true)
                findings.Add(new ValidationFinding(ValidationSeverity.Error, "TERMINAL001",
                    $"Terminal state '{transition.SourceState}' must not have outgoing transitions.",
                    transition.ToString(), "Remove the outgoing transition or do not mark the state terminal."));
        }

        foreach (var transition in definition.CompletionTransitions)
            foreach (var action in transition.TransitionActions)
            {
                if (string.IsNullOrWhiteSpace(action.DisplayName))
                    findings.Add(new ValidationFinding(ValidationSeverity.Error, "ACTION004",
                        "Transition action display name must be non-empty.",
                        $"completion:{transition}:action:{action.Order}"));

                if (action.Phase != TransitionLifecyclePhase.Transition)
                    findings.Add(new ValidationFinding(ValidationSeverity.Error, "ACTION005",
                        $"Transition action '{action.DisplayName}' has invalid phase '{action.Phase}'.",
                        $"completion:{transition}:action:{action.Order}"));
            }

        HierarchyStructureValidator.Validate(definition, findings);
        HierarchyCycleDetector.Validate(definition, findings);
        HierarchyInitialChildValidator.Validate(definition, findings);
        ParallelRegionStructureValidator.Validate(definition, findings);
        ParallelRegionMembershipValidator.Validate(definition, findings);
        ParallelRegionTransitionValidator.Validate(definition, findings);
        ParallelRegionAmbiguityValidator.Validate(definition, findings);
        HierarchyTransitionAmbiguityValidator.Validate(definition, findings);
        CompletionTransitionValidator.Validate(definition, findings);
        HierarchyCompletionValidator.Validate(definition, findings);
        HierarchyHistoryValidator.Validate(definition, findings);
        ParallelRegionHistoryValidator.Validate(definition, findings);

        if (definition.States.Count > 0)
        {
            var first = definition.States[0].Value;
            foreach (var state in definition.States)
            {
                if (comparer.Equals(state.Value, first)) continue;

                var hasIncoming = definition.Transitions.Any(t => comparer.Equals(t.TargetState, state.Value))
                                  || definition.CompletionTransitions.Any(t => comparer.Equals(t.TargetState, state.Value))
                                  || definition.States.Any(s =>
                                      s.Hierarchy.HasInitialChild &&
                                      comparer.Equals(s.Hierarchy.InitialChildState, state.Value));
                if (!hasIncoming)
                    findings.Add(new ValidationFinding(ValidationSeverity.Warning, "STATE003",
                        $"State '{state.Value}' appears unreachable because no transition targets it.",
                        $"state:{state.Value}", "Add a transition to this state or remove it if unused."));
            }
        }

        HierarchyReachabilityValidator.Validate(definition, findings);
        ParallelRegionReachabilityValidator.Validate(definition, findings);

        var conflictDiagnostics = TransitionConflictDiagnosticBuilder.BuildValidationDiagnostics(definition, findings);
        return new ValidationResult(findings, conflictDiagnostics);
    }

}