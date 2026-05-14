namespace StateForge.Core.Execution;

/// <summary>Serializes transition attempts for safe runtime contexts.</summary>
internal sealed class SerializedTransitionGate : IAsyncDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public ValueTask DisposeAsync()
    {
        _semaphore.Dispose();
        return ValueTask.CompletedTask;
    }

    public async ValueTask<IDisposable> EnterAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new Releaser(_semaphore);
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose()
        {
            semaphore.Release();
        }
    }
}