using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

/// <summary>Runs transition behaviors in lifecycle phase order.</summary>
internal sealed class TransitionBehaviorRunner<TState, TEvent>
{
    public async ValueTask RunPhaseAsync(
        TransitionDefinition<TState, TEvent> transition,
        TransitionLifecyclePhase phase,
        TransitionContext<TState, TEvent> context,
        CancellationToken cancellationToken)
    {
        foreach (var behavior in transition.Behaviors.Where(b => b.Phase == phase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await behavior.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }
}