using StateForge.Persistence.Snapshots;

namespace StateForge.Persistence.EntityFrameworkCore.Tests.TestSupport;

internal enum EfOrderState
{
    Draft,
    Submitted,
    Paid,
    Cancelled
}

internal static class EntityFrameworkCorePersistenceTestDomain
{
    public const string DefinitionId = "orders-v1";

    public static StateSnapshot<EfOrderState> Snapshot(
        string instanceId = "order-1",
        EfOrderState state = EfOrderState.Draft,
        long version = 1,
        string definitionId = DefinitionId,
        PersistencePropertyBag? properties = null)
    {
        return new StateSnapshot<EfOrderState>(
            instanceId,
            definitionId,
            state,
            PersistenceVersion.From(version, version.ToString()),
            properties);
    }
}
