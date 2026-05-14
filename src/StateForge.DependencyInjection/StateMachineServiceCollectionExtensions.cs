using Microsoft.Extensions.DependencyInjection;
using StateForge.Core.Definitions;
using StateForge.DependencyInjection.Registration;
using StateForge.DependencyInjection.Runtime;
using StateForge.DependencyInjection.Validation;

namespace StateForge.DependencyInjection;

public static class StateMachineServiceCollectionExtensions
{
    public static IServiceCollection AddStateMachines(this IServiceCollection services,
        Action<StateMachineRegistrationCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var collection = new StateMachineRegistrationCollection();
        configure(collection);
        services.AddSingleton(collection);
        services.AddSingleton<IStateMachineRuntimeFactoryResolver, StateMachineRuntimeFactoryResolver>();
        services.AddSingleton<IStateMachineRegistrationValidator, StateMachineRegistrationValidator>();
        services.AddSingleton(new StateMachineValidationOptions());
        return services;
    }

    public static StateMachineRegistration<TState, TEvent> AddDefinition<TState, TEvent>(
        this StateMachineRegistrationCollection machines,
        StateMachineDefinition<TState, TEvent> definition,
        Action<StateMachineRegistrationOptions<TState, TEvent>>? configure = null) =>
        machines.Add(definition, null, configure);

    public static StateMachineRegistration<TState, TEvent> AddDefinition<TState, TEvent>(
        this StateMachineRegistrationCollection machines,
        string name,
        StateMachineDefinition<TState, TEvent> definition,
        Action<StateMachineRegistrationOptions<TState, TEvent>>? configure = null) =>
        machines.Add(definition, name, configure);

    public static StateMachineRegistration<TState, TEvent> AddDefinition<TState, TEvent>(
        this StateMachineRegistrationCollection machines,
        string name,
        StateMachineDefinition<TState, TEvent> definition) =>
        machines.Add(definition, name, null);

    public static IStateMachineRuntimeFactory<TState, TEvent> GetRequiredStateMachineFactory<TState, TEvent>(
        this IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IStateMachineRuntimeFactoryResolver>().GetFactory<TState, TEvent>();

    public static IStateMachineRuntimeFactory<TState, TEvent> GetRequiredStateMachineFactory<TState, TEvent>(
        this IServiceProvider serviceProvider, string name) =>
        serviceProvider.GetRequiredService<IStateMachineRuntimeFactoryResolver>().GetFactory<TState, TEvent>(name);
}
