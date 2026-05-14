using Microsoft.Extensions.DependencyInjection;
using StateForge.Core.Execution;

namespace StateForge.DependencyInjection.Registration;

/// <summary>Describes an ordered observer attached to runtimes created from a registration.</summary>
public sealed class ObserverRegistration<TState, TEvent>
{
    private ObserverRegistration(int order, Type? serviceType, ITransitionObserver<TState, TEvent>? instance)
    {
        Order = order;
        ServiceType = serviceType;
        Instance = instance;
    }

    public int Order { get; }
    public Type? ServiceType { get; }
    public ITransitionObserver<TState, TEvent>? Instance { get; }

    public static ObserverRegistration<TState, TEvent> ForInstance(ITransitionObserver<TState, TEvent> observer, int order = 0) => new(order, null, observer);
    public static ObserverRegistration<TState, TEvent> ForService<TObserver>(int order = 0) where TObserver : ITransitionObserver<TState, TEvent> => new(order, typeof(TObserver), null);

    public ITransitionObserver<TState, TEvent> Resolve(IServiceProvider serviceProvider)
    {
        if (Instance is not null) return Instance;
        return (ITransitionObserver<TState, TEvent>)serviceProvider.GetRequiredService(ServiceType!);
    }
}
