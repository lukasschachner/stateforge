using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

/// <summary>Runs configured state and transition actions in deterministic lifecycle order.</summary>
internal sealed class TransitionActionRunner<TState, TEvent>
{
    public async ValueTask RunStateActionsAsync(
        StateDefinition<TState>? state,
        ActionKind kind,
        TransitionDefinition<TState, TEvent> transition,
        ActionExecutionContext<TState, TEvent> context,
        CancellationToken cancellationToken)
    {
        var actions = kind == ActionKind.Entry ? state?.EntryActions : state?.ExitActions;
        if (actions is null || actions.Count == 0) return;

        foreach (var action in actions.OrderBy(a => a.Order))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await action.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ActionExecutionException(action.Summary, transition, ex);
            }
        }
    }

    public async ValueTask RunTransitionActionsAsync(
        TransitionDefinition<TState, TEvent> transition,
        ActionExecutionContext<TState, TEvent> context,
        CancellationToken cancellationToken)
    {
        if (transition.TransitionActions.Count == 0) return;

        foreach (var action in transition.TransitionActions.OrderBy(a => a.Order))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await action.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ActionExecutionException(action.Summary, transition, ex);
            }
        }
    }
}