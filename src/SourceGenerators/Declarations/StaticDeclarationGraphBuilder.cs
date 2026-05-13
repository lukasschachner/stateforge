namespace StateMachineLibrary.SourceGenerators.Declarations;

public static class StaticDeclarationGraphBuilder
{
    public static StaticDeclarationGraph Build(MachineDeclaration declaration)
    {
        var stateKeys = new HashSet<string>(declaration.States.Select(s => s.IdentityKey), StringComparer.Ordinal);
        var nodes = DeclarationOrdering.States(declaration.States)
            .Select(s => new StaticDeclarationGraphNode(s.IdentityKey, s.Name, s.IsTerminal, s.SourceLocation))
            .ToArray();
        var edges = new List<StaticDeclarationGraphEdge>();

        foreach (var transition in DeclarationOrdering.Transitions(declaration.Transitions))
        {
            if (stateKeys.Contains(transition.SourceStateKey) && stateKeys.Contains(transition.TargetStateKey))
                edges.Add(new StaticDeclarationGraphEdge(transition.SourceStateKey, transition.TargetStateKey,
                    StaticDeclarationGraphEdgeKind.Transition, transition.SourceLocation));
        }

        foreach (var state in DeclarationOrdering.States(declaration.States))
        {
            if (state.InitialChildStateKey is not null && stateKeys.Contains(state.InitialChildStateKey))
                edges.Add(new StaticDeclarationGraphEdge(state.IdentityKey, state.InitialChildStateKey,
                    StaticDeclarationGraphEdgeKind.InitialChild, state.InitialChildLocation ?? state.SourceLocation));
            if (state.HistoryFallbackStateKey is not null && stateKeys.Contains(state.HistoryFallbackStateKey))
                edges.Add(new StaticDeclarationGraphEdge(state.IdentityKey, state.HistoryFallbackStateKey,
                    StaticDeclarationGraphEdgeKind.HistoryFallback, state.HistoryLocation ?? state.SourceLocation));
        }

        foreach (var completion in declaration.CompletionDeclarations.OrderBy(c => c.SourceStateKey, StringComparer.Ordinal)
                     .ThenBy(c => c.TargetStateKey, StringComparer.Ordinal))
        {
            if (stateKeys.Contains(completion.SourceStateKey) && stateKeys.Contains(completion.TargetStateKey))
                edges.Add(new StaticDeclarationGraphEdge(completion.SourceStateKey, completion.TargetStateKey,
                    StaticDeclarationGraphEdgeKind.Completion, completion.SourceLocation));
        }

        foreach (var region in declaration.Regions.OrderBy(r => r.OwnerCompositeStateKey, StringComparer.Ordinal)
                     .ThenBy(r => r.RegionName, StringComparer.Ordinal))
        {
            if (region.InitialStateKey is not null && stateKeys.Contains(region.OwnerCompositeStateKey) &&
                stateKeys.Contains(region.InitialStateKey))
                edges.Add(new StaticDeclarationGraphEdge(region.OwnerCompositeStateKey, region.InitialStateKey,
                    StaticDeclarationGraphEdgeKind.RegionInitial, region.InitialLocation ?? region.SourceLocation));
        }

        var roots = new List<string>();
        var firstState = declaration.States.FirstOrDefault();
        if (firstState is not null) roots.Add(firstState.IdentityKey);
        roots.AddRange(declaration.Regions.Select(r => r.InitialStateKey).Where(k => k is not null).Cast<string>()
            .Where(k => stateKeys.Contains(k)).OrderBy(k => k, StringComparer.Ordinal));

        return new StaticDeclarationGraph(nodes, edges, roots.Distinct(StringComparer.Ordinal).ToArray());
    }
}
