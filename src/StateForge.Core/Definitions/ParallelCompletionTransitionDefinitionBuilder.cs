using StateForge.Core.Execution;

namespace StateForge.Core.Definitions;

/// <summary>Fluent completion-transition configuration for a parallel composite state.</summary>
public sealed class ParallelCompletionTransitionDefinitionBuilder<TState, TEvent>
{
    private readonly List<TransitionBehaviorDefinition<TState, TEvent>> _behaviors = [];
    private readonly StateMachineDefinitionBuilder<TState, TEvent> _builder;
    private readonly List<ConditionDefinition<TState, TEvent>> _conditions = [];
    private readonly Dictionary<string, object?> _metadata = new(StringComparer.Ordinal);
    private readonly ParallelCompositeDefinitionBuilder<TState, TEvent> _parallelBuilder;
    private readonly List<TransitionActionDefinition<TState, TEvent>> _transitionActions = [];

    internal ParallelCompletionTransitionDefinitionBuilder(
        StateMachineDefinitionBuilder<TState, TEvent> builder,
        ParallelCompositeDefinitionBuilder<TState, TEvent> parallelBuilder)
    {
        _builder = builder;
        _parallelBuilder = parallelBuilder;
    }

    public ParallelCompletionTransitionDefinitionBuilder<TState, TEvent> When(
        Func<TransitionContext<TState, TEvent>, bool> condition,
        string? displayName = null)
    {
        ArgumentNullException.ThrowIfNull(condition);
        return WhenAsync((ctx, _) => ValueTask.FromResult(condition(ctx)), displayName);
    }

    public ParallelCompletionTransitionDefinitionBuilder<TState, TEvent> WhenAsync(
        Func<TransitionContext<TState, TEvent>, CancellationToken, ValueTask<bool>> condition,
        string? displayName = null)
    {
        ArgumentNullException.ThrowIfNull(condition);
        _conditions.Add(new ConditionDefinition<TState, TEvent>(condition, displayName));
        return this;
    }

    public ParallelCompletionTransitionDefinitionBuilder<TState, TEvent> OnExit(
        Action<TransitionContext<TState, TEvent>> behavior,
        string? displayName = null)
    {
        ArgumentNullException.ThrowIfNull(behavior);
        return OnExitAsync((ctx, _) =>
        {
            behavior(ctx);
            return ValueTask.CompletedTask;
        }, displayName);
    }

    public ParallelCompletionTransitionDefinitionBuilder<TState, TEvent> OnExitAsync(
        Func<TransitionContext<TState, TEvent>, CancellationToken, ValueTask> behavior,
        string? displayName = null)
    {
        return AddBehavior(TransitionLifecyclePhase.Exit, behavior, displayName);
    }

    public ParallelCompletionTransitionDefinitionBuilder<TState, TEvent> Execute(
        Action<TransitionContext<TState, TEvent>> behavior,
        string? displayName = null)
    {
        ArgumentNullException.ThrowIfNull(behavior);
        return ExecuteAsync((ctx, _) =>
        {
            behavior(ctx);
            return ValueTask.CompletedTask;
        }, displayName);
    }

    public ParallelCompletionTransitionDefinitionBuilder<TState, TEvent> ExecuteAsync(
        Func<TransitionContext<TState, TEvent>, CancellationToken, ValueTask> behavior,
        string? displayName = null)
    {
        ArgumentNullException.ThrowIfNull(behavior);
        var order = _transitionActions.Count;
        _transitionActions.Add(TransitionActionDefinition<TState, TEvent>.FromAsync(order,
            async (ctx, cancellationToken) =>
            {
                var legacyContext = new TransitionContext<TState, TEvent>(ctx.Definition, ctx.SourceState, ctx.Event,
                    ctx.CancellationToken, ctx.Transition, ctx.TargetState, ctx.Metadata, ctx.TriggerKind);
                await behavior(legacyContext, cancellationToken).ConfigureAwait(false);
            }, displayName));
        return this;
    }

    public ParallelCompletionTransitionDefinitionBuilder<TState, TEvent> ExecuteAction(
        Action<ActionExecutionContext<TState, TEvent>> action,
        string? displayName = null,
        MetadataCollection? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        var order = _transitionActions.Count;
        _transitionActions.Add(TransitionActionDefinition<TState, TEvent>.FromSync(order, action, displayName, metadata));
        return this;
    }

    public ParallelCompletionTransitionDefinitionBuilder<TState, TEvent> ExecuteActionAsync(
        Func<ActionExecutionContext<TState, TEvent>, CancellationToken, ValueTask> action,
        string? displayName = null,
        MetadataCollection? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        var order = _transitionActions.Count;
        _transitionActions.Add(TransitionActionDefinition<TState, TEvent>.FromAsync(order, action, displayName, metadata));
        return this;
    }

    public ParallelCompletionTransitionDefinitionBuilder<TState, TEvent> OnEntry(
        Action<TransitionContext<TState, TEvent>> behavior,
        string? displayName = null)
    {
        ArgumentNullException.ThrowIfNull(behavior);
        return OnEntryAsync((ctx, _) =>
        {
            behavior(ctx);
            return ValueTask.CompletedTask;
        }, displayName);
    }

    public ParallelCompletionTransitionDefinitionBuilder<TState, TEvent> OnEntryAsync(
        Func<TransitionContext<TState, TEvent>, CancellationToken, ValueTask> behavior,
        string? displayName = null)
    {
        return AddBehavior(TransitionLifecyclePhase.Entry, behavior, displayName);
    }

    public ParallelCompletionTransitionDefinitionBuilder<TState, TEvent> WithMetadata(string key, object? value)
    {
        _metadata[key] = value;
        return this;
    }

    public ParallelCompositeDefinitionBuilder<TState, TEvent> GoTo(TState targetState)
    {
        return Complete(targetState, TransitionKind.External);
    }

    public ParallelCompositeDefinitionBuilder<TState, TEvent> Self()
    {
        return Complete(_parallelBuilder.CompositeState, TransitionKind.Self);
    }

    public ParallelCompositeDefinitionBuilder<TState, TEvent> Internal()
    {
        return Complete(_parallelBuilder.CompositeState, TransitionKind.Internal);
    }

    private ParallelCompletionTransitionDefinitionBuilder<TState, TEvent> AddBehavior(
        TransitionLifecyclePhase phase,
        Func<TransitionContext<TState, TEvent>, CancellationToken, ValueTask> behavior,
        string? displayName)
    {
        ArgumentNullException.ThrowIfNull(behavior);
        _behaviors.Add(new TransitionBehaviorDefinition<TState, TEvent>(phase, behavior, displayName));
        return this;
    }

    private ParallelCompositeDefinitionBuilder<TState, TEvent> Complete(TState targetState, TransitionKind kind)
    {
        _builder.AddCompletionTransition(new CompletionTransitionDefinition<TState, TEvent>(
            _parallelBuilder.CompositeState,
            targetState,
            kind,
            _conditions,
            _behaviors,
            new MetadataCollection(_metadata),
            _transitionActions));
        return _parallelBuilder;
    }
}
