using StateMachineLibrary.Core.Definitions;

namespace StateMachineLibrary.Core.Execution;

internal sealed class CompletionEpisodeTracker<TState>
{
    private readonly Dictionary<StateKey<TState>, CompletionEpisodeStatus> _recognized = [];

    public bool IsRecognized(TState completionScope)
    {
        return _recognized.ContainsKey(new StateKey<TState>(completionScope));
    }

    public void MarkSelected(TState completionScope)
    {
        _recognized[new StateKey<TState>(completionScope)] = CompletionEpisodeStatus.Selected;
    }

    public void MarkNoEligible(TState completionScope)
    {
        _recognized[new StateKey<TState>(completionScope)] = CompletionEpisodeStatus.NoEligibleTransition;
    }

    public void ResetExitedScopes<TEvent>(StateMachineDefinition<TState, TEvent> definition,
        ActiveStateShape<TState> before,
        ActiveStateShape<TState> after)
    {
        if (_recognized.Count == 0) return;

        var activeScopes = new HashSet<StateKey<TState>>();
        if (after.IsParallel && after.OwningCompositeState is not null)
        {
            activeScopes.Add(new StateKey<TState>(after.OwningCompositeState));
            foreach (var region in after.ActiveRegions)
                foreach (var state in region.ActivePath.StatesRootToLeaf)
                    activeScopes.Add(new StateKey<TState>(state));
        }
        else if (after.ActiveLeafState is not null)
        {
            foreach (var state in definition.GetActiveStatePath(after.ActiveLeafState).StatesRootToLeaf)
                activeScopes.Add(new StateKey<TState>(state));
        }

        var exited = _recognized.Keys.Where(scope => !activeScopes.Contains(scope)).ToArray();
        foreach (var scope in exited) _recognized.Remove(scope);
    }

    public void Clear()
    {
        _recognized.Clear();
    }

    private readonly record struct StateKey<T>(T Value);
}

internal enum CompletionEpisodeStatus
{
    Selected,
    NoEligibleTransition
}
