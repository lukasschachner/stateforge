using StateForge.Core.Tests.Hierarchy;
using StateForge.Core.Definitions;
using StateForge.Core.Validation;

namespace StateForge.Core.Tests.Validation;

public class HierarchyReachabilityValidationTests
{
    [Fact]
    public void NestedStatesWithoutIncomingTransitionsOrInitialChainsAreReportedAsUnreachable()
    {
        var definition = StateMachineDefinition<HierarchyState, HierarchyEvent>.Create(builder =>
        {
            builder.State(HierarchyState.Reviewing).InitialChild(HierarchyState.AuthorReview);
            builder.State(HierarchyState.AuthorReview);
            builder.State(HierarchyState.LegalReview).ChildOf(HierarchyState.Reviewing);
        });

        var validation = definition.Validate();

        Assert.Contains(validation.Warnings,
            f => f.Code == HierarchyValidationCodes.UnreachableNestedState &&
                 (f.AffectedElement?.Contains(HierarchyState.LegalReview.ToString(), StringComparison.Ordinal) ??
                  false));
    }
}