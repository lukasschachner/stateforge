using Core.Tests.Execution;
using StateMachineLibrary.Core.Execution;

namespace StateMachineLibrary.Core.Tests.Hierarchy;

public sealed class ActiveStateSnapshotHierarchyTests
{
    [Fact]
    public void HierarchicalSnapshotCapturesOrderedRootToLeafPathAndRestoresLeaf()
    {
        var definition = ActiveStateSnapshotTestDomain.CreateHierarchicalDefinition();
        var runtime = definition.CreateRuntime(SnapshotState.Running);

        var snapshot = runtime.CaptureActiveStateSnapshot();
        var restored = definition.CreateRuntime(snapshot);

        Assert.Equal(ActiveStateSnapshotKind.Hierarchical, snapshot.Kind);
        ActiveStateSnapshotAssertions.AssertPath(snapshot.ActivePath,
            SnapshotState.Running,
            SnapshotState.RunningChild);
        Assert.Equal(SnapshotState.RunningChild, restored.CurrentState);
    }
}
