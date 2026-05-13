using Interactive.ApiFrontendSample.Features.OrderWorkflow;

if (args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase))
{
    await OrderWorkflowSmokeRunner.RunAsync();
    return;
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<OrderWorkflowRuntimeService>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapOrderWorkflowApi();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapFallbackToFile("index.html");

await app.RunAsync();
