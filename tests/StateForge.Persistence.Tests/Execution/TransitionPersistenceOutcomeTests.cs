using StateForge.Persistence.Tests.TestSupport;
using StateForge.Core.Execution;
using StateForge.Persistence.Execution;
using StateForge.Persistence.Snapshots;

namespace StateForge.Persistence.Tests.Execution;

public class TransitionPersistenceOutcomeTests
{
    [Fact]
    public void PersistedOutcomeIsCommitted()
    {
        var transition = TransitionOutcome<OrderState, OrderEvent>.Success(
            OrderState.Draft,
            OrderState.Submitted,
            new Submit(),
            PersistenceTestDomain.CreateDefinition().Transitions[0]);

        var committed = new CommittedSnapshot<OrderState>("order-1", PersistenceTestDomain.DefinitionId,
            OrderState.Submitted, PersistenceVersion.From("v2"));
        var outcome =
            TransitionPersistenceOutcome<OrderState, OrderEvent>.Persisted(transition, committed,
                PersistenceVersion.From("v1"));

        Assert.Equal(TransitionPersistenceCategory.Persisted, outcome.PersistenceCategory);
        Assert.Same(committed, outcome.CommittedSnapshot);
    }

    [Fact]
    public void ConcurrencyOutcomeIsUncommitted()
    {
        var transition = TransitionOutcome<OrderState, OrderEvent>.Success(
            OrderState.Draft,
            OrderState.Submitted,
            new Submit(),
            PersistenceTestDomain.CreateDefinition().Transitions[0]);

        var proposed = new ProposedSnapshot<OrderState, OrderEvent>(
            "order-1",
            PersistenceTestDomain.DefinitionId,
            OrderState.Submitted,
            PersistenceVersion.From("v2"),
            PersistenceVersion.From("v1"),
            transition);

        var outcome =
            TransitionPersistenceOutcome<OrderState, OrderEvent>.ConcurrentStateChange(transition, proposed,
                PersistenceVersion.From("v1"));

        Assert.Equal(TransitionPersistenceCategory.ConcurrentStateChange, outcome.PersistenceCategory);
        Assert.Same(proposed, outcome.ProposedSnapshot);
        Assert.Null(outcome.CommittedSnapshot);
    }
}