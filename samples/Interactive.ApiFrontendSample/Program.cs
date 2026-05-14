using Interactive.ApiFrontendSample.Features.OrderWorkflow;
using StateForge.DependencyInjection.Validation;

if (args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase))
{
    await OrderWorkflowSmokeRunner.RunAsync();
    return;
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOrderWorkflowDemo();

var app = builder.Build();
var validation = await app.Services.GetRequiredService<IStateMachineRegistrationValidator>().ValidateAsync();
if (!validation.Succeeded)
    throw new InvalidOperationException(validation.ToDisplayString());
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapOrderWorkflowApi();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapFallbackToFile("index.html");

await app.RunAsync();
