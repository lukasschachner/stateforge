using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StateForge.Core.Definitions;
using StateForge.Core.Execution;
using StateForge.DependencyInjection;
using StateForge.DependencyInjection.Runtime;
using StateForge.DependencyInjection.Validation;
using StateForge.Logging;

var definition = StateMachineDefinition<CheckoutState, CheckoutEvent>.Create(builder =>
{
    builder.State(CheckoutState.Cart).On(CheckoutEvent.Pay).GoTo(CheckoutState.Paid);
    builder.State(CheckoutState.Paid).Terminal();
});

var loggingOptions = StateMachineLoggingExtensions.AddStateMachineLogging(options =>
    options.IncludeTransitionSuccesses().IncludeTransitionDenials().UseDefaultSafeDiagnostics());

var services = new ServiceCollection();
services.AddLogging(builder => builder
    .SetMinimumLevel(LogLevel.Information)
    .AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
    }));

using var loggerFactory = LoggerFactory.Create(builder => builder
    .SetMinimumLevel(LogLevel.Information)
    .AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
    }));
var logger = loggerFactory.CreateLogger("ApplicationIntegration.Sample.StateMachines");

services.AddStateMachines(machines =>
{
    machines.AddDefinition("checkout", definition, machine => machine
        .ValidateOnStartup()
        .UseObserver(logger.CreateStateMachineLoggingObserver<CheckoutState, CheckoutEvent>(loggingOptions)));
    machines.AddDefinition(definition);
});

using var provider = services.BuildServiceProvider();
var validator = provider.GetRequiredService<IStateMachineRegistrationValidator>();
var validation = await validator.ValidateAsync();
if (!validation.Succeeded) throw new InvalidOperationException(validation.ToDisplayString());

var factory = provider.GetRequiredService<IStateMachineRuntimeFactoryResolver>()
    .GetFactory<CheckoutState, CheckoutEvent>("checkout");
var runtime = factory.Create(CheckoutState.Cart, ConcurrencyMode.Serialized);
var outcome = await runtime.ApplyAsync(CheckoutEvent.Pay);

Console.WriteLine($"Application integration sample completed: {outcome.Category} -> {runtime.CurrentState}");

internal enum CheckoutState { Cart, Paid }
internal enum CheckoutEvent { Pay }
