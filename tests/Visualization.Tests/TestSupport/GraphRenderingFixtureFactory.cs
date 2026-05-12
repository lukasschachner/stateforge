using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Introspection;
using StateMachineLibrary.Core.Validation;

namespace Visualization.Tests.TestSupport;

internal static class GraphRenderingFixtureFactory
{
    public static DefinitionGraph<string, string> CreateOrderFlowGraph()
    {
        var nodes = new[]
        {
            new GraphNode<string>("state-created", "Created", "Created", false,
                new MetadataCollection(new Dictionary<string, object?>
                    { ["beta"] = 2, ["alpha"] = 1, ["zeta"] = "last" })),
            new GraphNode<string>("state-paid", "Paid", "Paid", false,
                new MetadataCollection(new Dictionary<string, object?> { ["note"] = "captured" })),
            new GraphNode<string>("state-shipped", "Shipped", "Shipped", true,
                new MetadataCollection(new Dictionary<string, object?> { ["terminal"] = true })),
            new GraphNode<string>("state-cancelled", "Cancelled", "Cancelled", true,
                new MetadataCollection(new Dictionary<string, object?> { ["terminal"] = true }))
        };

        var edges = new[]
        {
            CreateEdge("transition-pay", "state-created", "state-paid", "Pay"),
            CreateEdge("transition-ship", "state-paid", "state-shipped", "Ship"),
            CreateEdge("transition-cancel-from-created", "state-created", "state-cancelled", "Cancel"),
            CreateEdge("transition-cancel-from-paid", "state-paid", "state-cancelled", "Cancel")
        };

        return new DefinitionGraph<string, string>(
            "order-flow",
            "Order flow",
            nodes,
            edges,
            new MetadataCollection(new Dictionary<string, object?>
            {
                ["owner"] = "docs",
                ["tags"] = new[] { "examples", "graph-rendering" },
                ["threshold"] = 1.5m,
                ["flags"] = new Dictionary<string, object?>
                {
                    ["beta"] = true,
                    ["alpha"] = false
                }
            }),
            ValidationResult.Valid);
    }

    public static DefinitionGraph<string, string> CreateHierarchyGraph()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.State("Draft").On("submit").GoTo("Reviewing");
            builder.State("Reviewing")
                .InitialChild("Author")
                .On("cancel").GoTo("Rejected");
            builder.State("Author").On("submit").GoTo("Legal");
            builder.State("Legal").ChildOf("Reviewing");
            builder.State("Rejected").Terminal();
        });

        return Assert.IsType<DefinitionGraph<string, string>>(definition.ExportGraph().Graph);
    }

    public static DefinitionGraph<string, string> CreateHistoryGraph()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.State("Draft").On("submit").GoTo("Reviewing");
            builder.State("Reviewing")
                .InitialChild("Author")
                .WithShallowHistory("Legal")
                .On("cancel").GoTo("Rejected");
            builder.State("Author").On("submit").GoTo("Legal");
            builder.State("Legal").ChildOf("Reviewing");
            builder.State("Rejected").Terminal();
        });

        return Assert.IsType<DefinitionGraph<string, string>>(definition.ExportGraph().Graph);
    }

    public static DefinitionGraph<string, string> CreateEquivalentOrderFlowGraphWithReorderedCollections()
    {
        var graph = CreateOrderFlowGraph();
        return new DefinitionGraph<string, string>(
            graph.Id,
            graph.Label,
            graph.Nodes.Reverse().ToArray(),
            graph.Edges.Reverse().ToArray(),
            graph.Metadata,
            graph.Validation);
    }

    public static DefinitionGraph<string, string> CreateReservedCharacterGraph()
    {
        var nodes = new[]
        {
            new GraphNode<string>("state \"A\"", "State \"A\"", "State \"A\"", false, MetadataCollection.Empty),
            new GraphNode<string>("state\\B", "State\\B", "State\\B", true, MetadataCollection.Empty)
        };

        var edge = new GraphEdge<string, string>(
            "edge/newline",
            nodes[0].Id,
            nodes[1].Id,
            nodes[0].State,
            nodes[1].State,
            "go -> next\nline [x]",
            new GraphEventDescriptor<string>("event:\"go\"", "Go [Action]",
                new MetadataCollection(new Dictionary<string, object?> { ["quote"] = "A\"B" }), "Value"),
            TransitionKind.External,
            new GraphConditionSummary<string, string>(
                GraphConditionSummaryKind.All,
                "if value has [brackets]",
                [new GraphConditionDescriptor(0, "condition[0]", MetadataCollection.Empty)],
                new MetadataCollection(new Dictionary<string, object?> { ["kind"] = "all" })),
            new MetadataCollection(new Dictionary<string, object?> { ["newline"] = "line1\nline2" }));

        return new DefinitionGraph<string, string>(
            "reserved/chars",
            "Reserved \"chars\"",
            nodes,
            [edge],
            new MetadataCollection(new Dictionary<string, object?> { ["kind"] = "reserved", ["weight"] = 7 }),
            ValidationResult.Valid);
    }

    public static DefinitionGraph<string, string> CreateSingleNodeGraph()
    {
        return new DefinitionGraph<string, string>(
            "single",
            "Single node",
            [new GraphNode<string>("state-only", "Only", "Only", true, MetadataCollection.Empty)],
            [],
            MetadataCollection.Empty,
            ValidationResult.Valid);
    }

    public static DefinitionGraph<string, string> CreateInvalidEdgeGraph()
    {
        return new DefinitionGraph<string, string>(
            "invalid",
            "Invalid",
            [new GraphNode<string>("state-a", "A", "A", false, MetadataCollection.Empty)],
            [
                new GraphEdge<string, string>(
                    "edge-a-b",
                    "state-a",
                    "state-missing",
                    "A",
                    "B",
                    "to missing",
                    new GraphEventDescriptor<string>("event:missing", "missing", MetadataCollection.Empty, "Value"),
                    TransitionKind.External,
                    new GraphConditionSummary<string, string>(GraphConditionSummaryKind.None, "No conditions", [],
                        MetadataCollection.Empty),
                    MetadataCollection.Empty)
            ],
            MetadataCollection.Empty,
            ValidationResult.Valid);
    }

    private static GraphEdge<string, string> CreateEdge(string id, string sourceNodeId, string targetNodeId,
        string label)
    {
        return new GraphEdge<string, string>(
            id,
            sourceNodeId,
            targetNodeId,
            sourceNodeId,
            targetNodeId,
            label,
            new GraphEventDescriptor<string>($"event:{label.ToLowerInvariant()}", label, MetadataCollection.Empty,
                "Value"),
            TransitionKind.External,
            new GraphConditionSummary<string, string>(GraphConditionSummaryKind.None, "No conditions", [],
                MetadataCollection.Empty),
            new MetadataCollection(new Dictionary<string, object?> { ["edge"] = id }));
    }
}