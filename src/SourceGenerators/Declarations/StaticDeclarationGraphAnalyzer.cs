using StateMachineLibrary.SourceGenerators.Diagnostics;

namespace StateMachineLibrary.SourceGenerators.Declarations;

public static class StaticDeclarationGraphAnalyzer
{
    public static void Analyze(StaticDeclarationGraph graph, DiagnosticReporter reporter)
    {
        if (graph.Nodes.Count == 0) return;

        var outgoing = graph.Edges.GroupBy(e => e.SourceStateKey, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.Ordinal);
        var reachable = Reachable(graph.InitialRoots, outgoing);

        foreach (var node in graph.Nodes.Where(n => !reachable.Contains(n.StateKey)))
            reporter.UnreachableState(node.DisplayName, node.Location);

        var terminalNodes = new HashSet<string>(graph.Nodes.Where(n => n.IsTerminal).Select(n => n.StateKey), StringComparer.Ordinal);
        if (terminalNodes.Count > 0)
        {
            foreach (var node in graph.Nodes.Where(n => !n.IsTerminal && reachable.Contains(n.StateKey)))
            {
                if (!outgoing.TryGetValue(node.StateKey, out var edges) || edges.Length == 0)
                    reporter.DeadEndState(node.DisplayName, node.Location);
            }
        }

        if (terminalNodes.Count > 0 && !terminalNodes.Overlaps(reachable))
        {
            var firstTerminal = graph.Nodes.First(n => terminalNodes.Contains(n.StateKey));
            reporter.TerminalNotReachable(string.Join(", ", graph.Nodes.Where(n => terminalNodes.Contains(n.StateKey))
                    .Select(n => n.DisplayName).OrderBy(n => n, StringComparer.Ordinal)),
                firstTerminal.Location);
        }
    }

    private static HashSet<string> Reachable(IEnumerable<string> roots,
        IReadOnlyDictionary<string, StaticDeclarationGraphEdge[]> outgoing)
    {
        var reachable = new HashSet<string>(StringComparer.Ordinal);
        var stack = new Stack<string>(roots.OrderByDescending(r => r, StringComparer.Ordinal));
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!reachable.Add(current)) continue;
            if (!outgoing.TryGetValue(current, out var edges)) continue;
            foreach (var target in edges.Select(e => e.TargetStateKey).OrderByDescending(k => k, StringComparer.Ordinal))
                stack.Push(target);
        }

        return reachable;
    }
}
