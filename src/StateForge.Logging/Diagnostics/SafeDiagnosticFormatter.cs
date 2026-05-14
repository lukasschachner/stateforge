using StateForge.Core.Execution;
using StateForge.Core.Validation;
using StateForge.Logging.Configuration;

namespace StateForge.Logging.Diagnostics;

public static class SafeDiagnosticFormatter
{
    private static readonly string[] SecretTokens = ["secret", "password", "token", "key", "credential"];

    public static string SafeValue(object? value, StateMachineLoggingOptions? options = null)
    {
        if (value is null) return string.Empty;
        var text = value.ToString() ?? string.Empty;
        if (SecretTokens.Any(t => text.Contains(t, StringComparison.OrdinalIgnoreCase))) return "[redacted]";
        var max = Math.Max(16, options?.MaxMetadataValueLength ?? 128);
        return text.Length <= max ? text : text[..max] + "…";
    }

    public static IReadOnlyList<string> DiagnosticCodes<TState, TEvent>(TransitionObservation<TState, TEvent> observation)
    {
        var codes = new List<string>();
        codes.AddRange(observation.Diagnostics.DenialDiagnostics.Select(d => d.Reason.ToString()));
        codes.AddRange(observation.Diagnostics.ConflictDiagnostics.Select(d => d.Kind.ToString()));
        codes.AddRange(observation.Diagnostics.ValidationFindings.Select(f => f.Code));
        return codes;
    }

    public static IReadOnlyList<string> DiagnosticCodes(ValidationFinding finding) => [finding.Code];
}
