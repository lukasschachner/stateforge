using StateForge.Core.Definitions;

namespace StateForge.Core.Validation;

internal static class ParallelRegionAmbiguityValidator
{
    public static void Validate<TState, TEvent>(StateMachineDefinition<TState, TEvent> definition,
        ICollection<ValidationFinding> findings)
    {
        foreach (var group in definition.Transitions.GroupBy(t => (t.SourceState, t.Event.Identity)))
            if (group.Count() > 1)
                findings.Add(new ValidationFinding(ValidationSeverity.Error, ParallelValidationCodes.AmbiguousEvent,
                    $"State '{group.Key.SourceState}' has multiple transitions for event '{group.Key.Identity}'.",
                    $"state:{group.Key.SourceState}:event:{group.Key.Identity}",
                    "Make transition selection deterministic by using one transition per source/event."));
    }
}