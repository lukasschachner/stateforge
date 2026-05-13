namespace Interactive.ApiFrontendSample.Features.OrderWorkflow;

internal static class OrderWorkflowApi
{
    public static IEndpointRouteBuilder MapOrderWorkflowApi(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/order-workflow");

        group.MapGet("/events/catalog", (OrderWorkflowRuntimeService service) => Results.Ok(service.GetEventCatalog()));

        group.MapGet("/runtime/state", async (OrderWorkflowRuntimeService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetRuntimeStateAsync(cancellationToken).ConfigureAwait(false)));

        group.MapGet("/definition/graph", async (OrderWorkflowRuntimeService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetGraphAsync(cancellationToken).ConfigureAwait(false)));

        group.MapGet("/definition/diagram/mermaid",
            async (OrderWorkflowRuntimeService service, CancellationToken cancellationToken) =>
                Results.Ok(await service.GetMermaidDiagramAsync(cancellationToken).ConfigureAwait(false)));

        group.MapPost("/runtime/events/preview",
            async (DemoEventRequest request, OrderWorkflowRuntimeService service, CancellationToken cancellationToken) =>
            {
                if (!OrderWorkflowEvents.TryCreate(request, out var @event, out var error))
                    return Results.BadRequest(new { error });

                var preview = await service.PreviewAsync(@event!, cancellationToken).ConfigureAwait(false);
                return Results.Ok(preview);
            });

        group.MapPost("/runtime/events/apply",
            async (DemoEventRequest request, OrderWorkflowRuntimeService service, CancellationToken cancellationToken) =>
            {
                if (!OrderWorkflowEvents.TryCreate(request, out var @event, out var error))
                    return Results.BadRequest(new { error });

                var outcome = await service.ApplyAsync(@event!, cancellationToken).ConfigureAwait(false);
                return Results.Ok(outcome);
            });

        group.MapPost("/runtime/reset", async (OrderWorkflowRuntimeService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ResetAsync(cancellationToken).ConfigureAwait(false)));

        return endpoints;
    }
}
