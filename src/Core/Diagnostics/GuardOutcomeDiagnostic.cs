namespace StateMachineLibrary.Core.Diagnostics;

/// <summary>Safe guard evaluation context for a transition participant.</summary>
public sealed class GuardOutcomeDiagnostic
{
    public GuardOutcomeDiagnostic(bool wasEnabled, IEnumerable<string>? conditionSummaries = null, string? transitionId = null)
    {
        WasEnabled = wasEnabled;
        ConditionSummaries = (conditionSummaries ?? []).ToArray();
        TransitionId = transitionId;
    }

    public bool WasEnabled { get; }
    public IReadOnlyList<string> ConditionSummaries { get; }
    public string? TransitionId { get; }
}
