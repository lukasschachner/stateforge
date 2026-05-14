using StateForge.Core.Validation;

namespace StateForge.Core.Introspection;

/// <summary>Represents the outcome of exporting a reusable state machine definition as graph data.</summary>
/// <typeparam name="TState">The state value type used by the definition.</typeparam>
/// <typeparam name="TEvent">The event value type used by the definition.</typeparam>
public sealed class GraphExportResult<TState, TEvent>
{
    private GraphExportResult(bool succeeded, DefinitionGraph<TState, TEvent>? graph, ValidationResult validation,
        string? failureSummary)
    {
        Succeeded = succeeded;
        Graph = graph;
        Validation = validation;
        FailureSummary = failureSummary;
    }

    /// <summary>Gets a value indicating whether graph export succeeded.</summary>
    public bool Succeeded { get; }

    /// <summary>Gets the exported graph when <see cref="Succeeded" /> is true; otherwise <see langword="null" />.</summary>
    public DefinitionGraph<TState, TEvent>? Graph { get; }

    /// <summary>Gets validation findings observed while exporting, including warnings on successful exports.</summary>
    public ValidationResult Validation { get; }

    /// <summary>Gets a human-readable explanation when export is refused; otherwise <see langword="null" />.</summary>
    public string? FailureSummary { get; }

    /// <summary>Creates a successful graph export result.</summary>
    public static GraphExportResult<TState, TEvent> Success(DefinitionGraph<TState, TEvent> graph,
        ValidationResult validation)
    {
        return new GraphExportResult<TState, TEvent>(true, graph ?? throw new ArgumentNullException(nameof(graph)),
            validation ?? throw new ArgumentNullException(nameof(validation)), null);
    }

    /// <summary>Creates a failed graph export result with validation findings and no partial graph data.</summary>
    public static GraphExportResult<TState, TEvent> Failure(ValidationResult validation)
    {
        ArgumentNullException.ThrowIfNull(validation);
        var errorCount = validation.Errors.Count;
        var summary = errorCount == 1
            ? "Graph export refused because validation produced 1 error."
            : $"Graph export refused because validation produced {errorCount} errors.";
        return new GraphExportResult<TState, TEvent>(false, null, validation, summary);
    }
}