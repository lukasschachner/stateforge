namespace StateForge.Core.Definitions;

/// <summary>Well-known metadata keys used by parallel-region graph and introspection data.</summary>
public static class ParallelRegionMetadataKeys
{
    public const string IsParallelComposite = "parallel.isComposite";
    public const string RegionId = "parallel.regionId";
    public const string RegionName = "parallel.regionName";
    public const string RegionOrder = "parallel.regionOrder";
    public const string OwnerCompositeState = "parallel.ownerCompositeState";
}