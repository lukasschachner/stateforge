using StateMachineLibrary.Core.Execution;

namespace StateMachineLibrary.Core.Definitions;

/// <summary>Fluent transition configuration.</summary>
public sealed class TransitionDefinitionBuilder<TState, TEvent>
{
    private readonly List<TransitionBehaviorDefinition<TState, TEvent>> _behaviors = [];
    private readonly StateMachineDefinitionBuilder<TState, TEvent> _builder;
    private readonly List<ConditionDefinition<TState, TEvent>> _conditions = [];
    private readonly EventDefinition<TEvent> _eventDefinition;
    private readonly Dictionary<string, object?> _metadata = new(StringComparer.Ordinal);
    private readonly StateDefinitionBuilder<TState, TEvent> _stateBuilder;
    private readonly List<TransitionActionDefinition<TState, TEvent>> _transitionActions = [];

    internal TransitionDefinitionBuilder(
        StateMachineDefinitionBuilder<TState, TEvent> builder,
        StateDefinitionBuilder<TState, TEvent> stateBuilder,
        EventDefinition<TEvent> eventDefinition)
    {
        _builder = builder;
        _stateBuilder = stateBuilder;
        _eventDefinition = eventDefinition;
    }

    public TransitionDefinitionBuilder<TState, TEvent> When(Func<TransitionContext<TState, TEvent>, bool> condition,
        string? displayName = null)
    {
        return WhenAsync(
            (Func<TransitionContext<TState, TEvent>, CancellationToken, ValueTask<bool>>)((ctx, _) =>
                ValueTask.FromResult(condition(ctx))), displayName);
    }

    public TransitionDefinitionBuilder<TState, TEvent> WhenAsync(
        Func<TransitionContext<TState, TEvent>, CancellationToken, ValueTask<bool>> condition,
        string? displayName = null)
    {
        _conditions.Add(new ConditionDefinition<TState, TEvent>(condition, displayName));
        return this;
    }

    public TransitionDefinitionBuilder<TState, TEvent> OnExit(Action<TransitionContext<TState, TEvent>> behavior,
        string? displayName = null)
    {
        return OnExitAsync((ctx, _) =>
        {
            behavior(ctx);
            return ValueTask.CompletedTask;
        }, displayName);
    }

    public TransitionDefinitionBuilder<TState, TEvent> OnExitAsync(
        Func<TransitionContext<TState, TEvent>, CancellationToken, ValueTask> behavior, string? displayName = null)
    {
        return AddBehavior(TransitionLifecyclePhase.Exit, behavior, displayName);
    }

    public TransitionDefinitionBuilder<TState, TEvent> Execute(Action<TransitionContext<TState, TEvent>> behavior,
        string? displayName = null)
    {
        ArgumentNullException.ThrowIfNull(behavior);
        return ExecuteAsync((ctx, _) =>
        {
            behavior(ctx);
            return ValueTask.CompletedTask;
        }, displayName);
    }

    public TransitionDefinitionBuilder<TState, TEvent> ExecuteAsync(
        Func<TransitionContext<TState, TEvent>, CancellationToken, ValueTask> behavior, string? displayName = null)
    {
        ArgumentNullException.ThrowIfNull(behavior);
        var order = _transitionActions.Count;
        _transitionActions.Add(TransitionActionDefinition<TState, TEvent>.FromAsync(order,
            async (ctx, cancellationToken) =>
            {
                var legacyContext = new TransitionContext<TState, TEvent>(ctx.Definition, ctx.SourceState, ctx.Event,
                    ctx.CancellationToken, ctx.Transition, ctx.TargetState, ctx.Metadata);
                await behavior(legacyContext, cancellationToken).ConfigureAwait(false);
            }, displayName));
        return this;
    }

    public TransitionDefinitionBuilder<TState, TEvent> ExecuteAction(
        Action<ActionExecutionContext<TState, TEvent>> action, string? displayName = null,
        MetadataCollection? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        var order = _transitionActions.Count;
        _transitionActions.Add(
            TransitionActionDefinition<TState, TEvent>.FromSync(order, action, displayName, metadata));
        return this;
    }

    public TransitionDefinitionBuilder<TState, TEvent> ExecuteActionAsync(
        Func<ActionExecutionContext<TState, TEvent>, CancellationToken, ValueTask> action, string? displayName = null,
        MetadataCollection? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        var order = _transitionActions.Count;
        _transitionActions.Add(
            TransitionActionDefinition<TState, TEvent>.FromAsync(order, action, displayName, metadata));
        return this;
    }

    public TransitionDefinitionBuilder<TState, TEvent> OnEntry(Action<TransitionContext<TState, TEvent>> behavior,
        string? displayName = null)
    {
        return OnEntryAsync((ctx, _) =>
        {
            behavior(ctx);
            return ValueTask.CompletedTask;
        }, displayName);
    }

    public TransitionDefinitionBuilder<TState, TEvent> OnEntryAsync(
        Func<TransitionContext<TState, TEvent>, CancellationToken, ValueTask> behavior, string? displayName = null)
    {
        return AddBehavior(TransitionLifecyclePhase.Entry, behavior, displayName);
    }

    public TransitionDefinitionBuilder<TState, TEvent> WithMetadata(string key, object? value)
    {
        _metadata[key] = value;
        return this;
    }

    public StateDefinitionBuilder<TState, TEvent> GoTo(TState targetState)
    {
        return Complete(targetState, TransitionKind.External);
    }

    public StateDefinitionBuilder<TState, TEvent> Self()
    {
        return Complete(_stateBuilder.State, TransitionKind.Self);
    }

    public StateDefinitionBuilder<TState, TEvent> Internal()
    {
        return Complete(_stateBuilder.State, TransitionKind.Internal);
    }

    private TransitionDefinitionBuilder<TState, TEvent> AddBehavior(
        TransitionLifecyclePhase phase,
        Func<TransitionContext<TState, TEvent>, CancellationToken, ValueTask> behavior,
        string? displayName)
    {
        _behaviors.Add(new TransitionBehaviorDefinition<TState, TEvent>(phase, behavior, displayName));
        return this;
    }

    private StateDefinitionBuilder<TState, TEvent> Complete(TState targetState, TransitionKind kind)
    {
        _builder.AddTransition(new TransitionDefinition<TState, TEvent>(
            _stateBuilder.State,
            _eventDefinition,
            targetState,
            kind,
            _conditions,
            _behaviors,
            new MetadataCollection(_metadata),
            _transitionActions));
        return _stateBuilder;
    }
}