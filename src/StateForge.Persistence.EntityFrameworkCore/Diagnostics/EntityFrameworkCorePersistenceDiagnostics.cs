using Microsoft.EntityFrameworkCore;
using StateForge.Persistence.Diagnostics;

namespace StateForge.Persistence.EntityFrameworkCore.Diagnostics;

internal static class EntityFrameworkCorePersistenceDiagnostics
{
    public static PersistenceDiagnostics InvalidInput(string summary, string code, string? affectedElement = null)
    {
        return new PersistenceDiagnostics(summary, code: code, affectedElement: affectedElement);
    }

    public static PersistenceDiagnostics InvalidSnapshot(string summary, string code, string? affectedElement = null)
    {
        return new PersistenceDiagnostics(summary, code: code, affectedElement: affectedElement);
    }

    public static PersistenceDiagnostics Cancelled(string operation)
    {
        return new PersistenceDiagnostics($"EF Core snapshot {operation} was cancelled.", code: "efcore.cancelled");
    }

    public static PersistenceDiagnostics ConcurrentStateChange(long? currentVersion = null)
    {
        var summary = currentVersion is null
            ? "Stored snapshot version changed before commit."
            : $"Stored snapshot version changed before commit. Current version hint: {currentVersion}.";

        return new PersistenceDiagnostics(summary, code: "efcore.concurrent-state-change");
    }

    public static PersistenceDiagnostics StorageFailure(string operation, Exception exception)
    {
        var code = exception switch
        {
            DbUpdateConcurrencyException => "efcore.concurrent-state-change",
            DbUpdateException => "efcore.db-update-failure",
            InvalidOperationException => "efcore.invalid-operation",
            _ => "efcore.storage-failure"
        };

        // Keep exception type only; do not leak provider payload / connection strings / stack traces.
        var safeSummary = $"EF Core snapshot {operation} failed ({exception.GetType().Name}).";
        return new PersistenceDiagnostics(safeSummary, code: code);
    }
}
