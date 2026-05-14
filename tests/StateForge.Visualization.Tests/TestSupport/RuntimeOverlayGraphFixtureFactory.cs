using StateForge.Core.Definitions;
using StateForge.Core.Introspection;

namespace StateForge.Visualization.Tests.TestSupport;

internal static class RuntimeOverlayGraphFixtureFactory
{
    public static DefinitionGraph<string, string> CreateFlatRuntimeOverlayGraph()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.State("Created").On("pay").GoTo("Paid");
            builder.State("Paid").Terminal();
        });

        return Assert.IsType<DefinitionGraph<string, string>>(definition.CreateRuntime("Created").ExportGraph().Graph);
    }

    public static DefinitionGraph<string, string> CreateHierarchyRuntimeOverlayGraph()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.CompositeState("Reviewing", "Author");
            builder.ChildState("Legal", "Reviewing");
            builder.State("Author").On("approve").GoTo("Legal");
        });

        return Assert.IsType<DefinitionGraph<string, string>>(definition.CreateRuntime("Reviewing").ExportGraph().Graph);
    }

    public static DefinitionGraph<string, string> CreateParallelRuntimeOverlayGraph()
    {
        var definition = StateMachineDefinition<string, string>.Create(builder =>
        {
            builder.ParallelComposite("Operational")
                .Region("Fulfillment", "WaitingForPick", ["WaitingForPick", "FulfillmentDone"], ["FulfillmentDone"])
                .Region("Billing", "WaitingForPayment", ["WaitingForPayment", "BillingDone"], ["BillingDone"]);
        });

        return Assert.IsType<DefinitionGraph<string, string>>(definition.CreateRuntime("Operational").ExportGraph().Graph);
    }
}
