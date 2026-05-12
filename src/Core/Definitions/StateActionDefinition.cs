using StateMachineLibrary.Core.Execution;

namespace StateMachineLibrary.Core.Definitions;

/// <summary>Executable action configured on a state entry or exit lifecycle phase.</summary>
public sealed class StateActionDefinition<TState>
{
    private readonly Func<object, CancellationToken, ValueTask> _executeAsync;

    public StateActionDefinition(
        ActionKind kind,
        int order,
        string displayName,
        Func<object, CancellationToken, ValueTask> executeAsync,
        MetadataCollection? metadata = null)
    {
        if (kind is not (ActionKind.Entry or ActionKind.Exit))
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "State actions must be Entry or Exit actions.");

        Kind = kind;
        Phase = kind == ActionKind.Entry ? TransitionLifecyclePhase.Entry : TransitionLifecyclePhase.Exit;
        Order = order < 0
            ? throw new ArgumentOutOfRangeException(nameof(order), "Action order must be non-negative.")
            : order;
        DisplayName = NormalizeDisplayName(displayName, kind, order);
        _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        Metadata = metadata ?? MetadataCollection.Empty;
    }

    public ActionKind Kind { get; }
    public TransitionLifecyclePhase Phase { get; }
    public int Order { get; }
    public string DisplayName { get; }
    public MetadataCollection Metadata { get; }
    public ActionSummary Summary => ActionSummary.From(this);

    public ValueTask ExecuteAsync<TEvent>(ActionExecutionContext<TState, TEvent> context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        return _executeAsync(context, cancellationToken);
    }

    public static StateActionDefinition<TState> FromSync<TEvent>(
        ActionKind kind,
        int order,
        Action<ActionExecutionContext<TState, TEvent>> execute,
        string? displayName = null,
        MetadataCollection? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(execute);
        return FromAsync<TEvent>(kind, order, (ctx, _) =>
        {
            execute(ctx);
            return ValueTask.CompletedTask;
        }, displayName, metadata);
    }

    public static StateActionDefinition<TState> FromAsync<TEvent>(
        ActionKind kind,
        int order,
        Func<ActionExecutionContext<TState, TEvent>, CancellationToken, ValueTask> executeAsync,
        string? displayName = null,
        MetadataCollection? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(executeAsync);
        var name = NormalizeDisplayName(displayName, kind, order);
        return new StateActionDefinition<TState>(kind, order, name, async (context, cancellationToken) =>
        {
            if (context is not ActionExecutionContext<TState, TEvent> typedContext)
                throw new InvalidOperationException(
                    "Action execution context type does not match the state machine definition.");

            await executeAsync(typedContext, cancellationToken).ConfigureAwait(false);
        }, metadata);
    }

    private static string NormalizeDisplayName(string? displayName, ActionKind kind, int order)
    {
        var normalized = string.IsNullOrWhiteSpace(displayName) ? $"{kind} action {order + 1}" : displayName.Trim();
        return normalized.Length == 0
            ? throw new ArgumentException("Action display name must be non-empty.", nameof(displayName))
            : normalized;
    }
}