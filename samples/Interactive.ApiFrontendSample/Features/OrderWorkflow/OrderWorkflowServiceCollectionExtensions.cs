using StateForge.DependencyInjection;

namespace Interactive.ApiFrontendSample.Features.OrderWorkflow;

internal static class OrderWorkflowServiceCollectionExtensions
{
    public static IServiceCollection AddOrderWorkflowDemo(this IServiceCollection services)
    {
        services.AddStateMachines(machines =>
        {
            machines.AddDefinition("interactive-order-workflow", OrderWorkflowDefinition.Create(), options =>
                options.ValidateOnStartup());
        });
        services.AddSingleton<OrderWorkflowRuntimeService>();
        return services;
    }
}
