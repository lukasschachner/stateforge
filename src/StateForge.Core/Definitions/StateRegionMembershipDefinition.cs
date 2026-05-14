namespace StateForge.Core.Definitions;

/// <summary>Identifies the parallel region that owns a state under a parallel composite.</summary>
public sealed record StateRegionMembershipDefinition<TState>(
    TState State,
    string RegionId,
    TState OwnerCompositeState,
    int Depth);