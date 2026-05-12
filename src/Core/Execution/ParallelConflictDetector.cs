using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

internal static class ParallelConflictDetector
{
    public static IReadOnlyList<TransitionDiagnostics> Detect<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        IEnumerable<TransitionDefinition<TState, TEvent>> transitions,
        TransitionDefinition<TState, TEvent>? parentTransition = null, bool parentIsCompletion = false)
    {
        var transitionArray = transitions.ToArray();
        var conflicts = new List<TransitionDiagnostics>();
        if (parentTransition is not null && transitionArray.Length > 0 && !parentIsCompletion)
            conflicts.Add(new TransitionDiagnostics(
                "Parent-level transition conflicts with selected regional transitions for the same dispatch.",
                TransitionLifecyclePhase.Matching, affectedElement: parentTransition.ToString(),
                parallelMetadata: new ParallelConflictMetadata(parentTransition.ToString(),
                    transitionArray.Select(t => t.ToString()).ToArray())));

        foreach (var duplicateSource in transitionArray.GroupBy(t => t.SourceState).Where(g => g.Count() > 1))
            conflicts.Add(new TransitionDiagnostics(
                $"Multiple transitions selected for source scope '{duplicateSource.Key}'.",
                TransitionLifecyclePhase.Matching, affectedElement: duplicateSource.Key?.ToString()));

        foreach (var transition in transitionArray)
            if (definition.TryGetCommonParallelOwner(transition.SourceState, transition.TargetState, out _,
                    out var sourceRegionId, out var targetRegionId)
                && !StringComparer.Ordinal.Equals(sourceRegionId, targetRegionId))
                conflicts.Add(new TransitionDiagnostics(
                    $"Transition '{transition}' crosses parallel region boundaries.", TransitionLifecyclePhase.Matching,
                    affectedElement: transition.ToString(),
                    parallelMetadata: new ParallelConflictMetadata(transition.ToString(),
                        [sourceRegionId ?? string.Empty, targetRegionId ?? string.Empty])));

        return conflicts;
    }
}

internal sealed record ParallelConflictMetadata(string Conflict, IReadOnlyList<string> Participants);