using StateMachineLibrary.Core.Execution;

namespace StateMachineLibrary.Core.Definitions;

/// <summary>Executable action configured on a transition lifecycle phase.</summary>
public sealed class TransitionActionDefinition<TState, TEvent>
{
    public TransitionActionDefinition(
        int order,
        string displayName,
        Func<ActionExecutionContext<TState, TEvent>, CancellationToken, ValueTask> executeAsync,
        MetadataCollection? metadata = null)
    {
        Order = order < 0
            ? throw new ArgumentOutOfRangeException(nameof(order), "Action order must be non-negative.")
            : order;
        DisplayName = NormalizeDisplayName(displayName, order);
        ExecuteAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        Metadata = metadata ?? MetadataCollection.Empty;
    }

    public ActionKind Kind => ActionKind.Transition;
    public TransitionLifecyclePhase Phase => TransitionLifecyclePhase.Transition;
    public int Order { get; }
    public string DisplayName { get; }
    public MetadataCollection Metadata { get; }
    public Func<ActionExecutionContext<TState, TEvent>, CancellationToken, ValueTask> ExecuteAsync { get; }
    public ActionSummary Summary => ActionSummary.From(this);

    public static TransitionActionDefinition<TState, TEvent> FromSync(
        int order,
        Action<ActionExecutionContext<TState, TEvent>> execute,
        string? displayName = null,
        MetadataCollection? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(execute);
        return FromAsync(order, (ctx, _) =>
        {
            execute(ctx);
            return ValueTask.CompletedTask;
        }, displayName, metadata);
    }

    public static TransitionActionDefinition<TState, TEvent> FromAsync(
        int order,
        Func<ActionExecutionContext<TState, TEvent>, CancellationToken, ValueTask> executeAsync,
        string? displayName = null,
        MetadataCollection? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(executeAsync);
        return new TransitionActionDefinition<TState, TEvent>(order, NormalizeDisplayName(displayName, order),
            executeAsync, metadata);
    }

    private static string NormalizeDisplayName(string? displayName, int order)
    {
        var normalized = string.IsNullOrWhiteSpace(displayName) ? $"Transition action {order + 1}" : displayName.Trim();
        return normalized.Length == 0
            ? throw new ArgumentException("Action display name must be non-empty.", nameof(displayName))
            : normalized;
    }
}