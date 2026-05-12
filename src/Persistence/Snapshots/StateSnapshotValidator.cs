using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Persistence.Snapshots;

/// <summary>Shared validation helper for state snapshot compatibility checks.</summary>
public static class StateSnapshotValidator
{
    public static SnapshotValidationResult Validate<TState, TEvent>(
        StateMachineDefinition<TState, TEvent> definition,
        StateSnapshot<TState> snapshot,
        string expectedDefinitionId)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(snapshot);

        var issues = new List<SnapshotValidationIssue>();

        if (string.IsNullOrWhiteSpace(snapshot.InstanceId))
            issues.Add(new SnapshotValidationIssue("snapshot.instance-id-required", "Snapshot instance id is required.",
                nameof(snapshot.InstanceId)));

        if (string.IsNullOrWhiteSpace(snapshot.DefinitionId))
            issues.Add(new SnapshotValidationIssue("snapshot.definition-id-required",
                "Snapshot definition id is required.", nameof(snapshot.DefinitionId)));
        else if (!string.Equals(snapshot.DefinitionId, expectedDefinitionId, StringComparison.Ordinal))
            issues.Add(new SnapshotValidationIssue(
                "snapshot.definition-id-mismatch",
                $"Snapshot definition '{snapshot.DefinitionId}' does not match expected definition '{expectedDefinitionId}'.",
                nameof(snapshot.DefinitionId)));

        if (snapshot.Version.Value is null)
            issues.Add(new SnapshotValidationIssue("snapshot.version-required", "Snapshot version is required.",
                nameof(snapshot.Version)));

        if (!definition.ContainsState(snapshot.ActiveState))
            issues.Add(new SnapshotValidationIssue(
                "snapshot.unknown-state",
                "Snapshot active state is not declared by the supplied machine definition.",
                nameof(snapshot.ActiveState)));

        return SnapshotValidationResult.FromIssues(issues);
    }
}