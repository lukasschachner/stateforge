namespace StateForge.Core.Execution;

/// <summary>Application-owned state access abstraction for external-state runtimes.</summary>
public interface IStateAccessor<TState>
{
    ValueTask<TState> GetAsync(CancellationToken cancellationToken = default);
    ValueTask SetAsync(TState state, CancellationToken cancellationToken = default);
}

/// <summary>Factory helpers for external state accessors.</summary>
public static class StateAccessor
{
    public static IStateAccessor<TState> Create<TState>(
        Func<TState> get,
        Action<TState> set)
    {
        return new DelegateStateAccessor<TState>(
            _ => ValueTask.FromResult(get()),
            (state, _) =>
            {
                set(state);
                return ValueTask.CompletedTask;
            });
    }

    public static IStateAccessor<TState> Create<TState>(
        Func<CancellationToken, ValueTask<TState>> getAsync,
        Func<TState, CancellationToken, ValueTask> setAsync)
    {
        return new DelegateStateAccessor<TState>(getAsync, setAsync);
    }

    private sealed class DelegateStateAccessor<TState>(
        Func<CancellationToken, ValueTask<TState>> getAsync,
        Func<TState, CancellationToken, ValueTask> setAsync) : IStateAccessor<TState>
    {
        public ValueTask<TState> GetAsync(CancellationToken cancellationToken = default)
        {
            return getAsync(cancellationToken);
        }

        public ValueTask SetAsync(TState state, CancellationToken cancellationToken = default)
        {
            return setAsync(state, cancellationToken);
        }
    }
}