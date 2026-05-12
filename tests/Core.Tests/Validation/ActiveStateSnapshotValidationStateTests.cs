using Core.Tests.Execution;
using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Core.Validation;

namespace Core.Tests.Validation;

public sealed class ActiveStateSnapshotValidationStateTests
{
    [Fact]
    public void ValidationReportsUnknownActiveLeafState()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateFlatDefinition();
        var snapshot = ActiveStateSnapshot<SnapshotState>.SingleLeaf(SnapshotState.Other);

        var result = definition.ValidateActiveStateSnapshot(snapshot);

        Assert.Contains(result.Diagnostics,
            diagnostic => diagnostic.Code == ActiveStateSnapshotValidationCodes.UnknownState &&
                          diagnostic.ReferencedState == SnapshotState.Other);
    }

    [Fact]
    public void ValidationReportsInvalidHierarchicalPath()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateHierarchicalDefinition();
        var snapshot = ActiveStateSnapshot<SnapshotState>.Hierarchical(
            SnapshotState.RunningChild,
            new ActiveStatePath<SnapshotState>([SnapshotState.RunningChild]));

        var result = definition.ValidateActiveStateSnapshot(snapshot);

        Assert.Contains(result.Diagnostics,
            diagnostic => diagnostic.Code == ActiveStateSnapshotValidationCodes.InvalidPath);
    }
}
