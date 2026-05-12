using Core.Tests.Hierarchy;
using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Validation;

namespace Core.Tests.Validation;

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