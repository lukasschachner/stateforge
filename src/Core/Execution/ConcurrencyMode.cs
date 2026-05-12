namespace StateMachineLibrary.Core.Execution;

/// <summary>Controls overlapping transition attempts for a runtime context.</summary>
public enum ConcurrencyMode
{
    /// <summary>Minimal synchronization. Callers coordinate concurrent access.</summary>
    Fast,

    /// <summary>Serializes overlapping transition attempts on a runtime context.</summary>
    Serialized
}