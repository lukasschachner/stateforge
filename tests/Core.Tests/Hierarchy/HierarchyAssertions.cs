using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Core.Validation;

namespace Core.Tests.Hierarchy;

public static class HierarchyAssertions
{
    public static void ActivePathIs<TState>(ActiveStatePath<TState> path, params TState[] expected)
    {
        Assert.Equal(expected, path.StatesRootToLeaf);
    }

    public static void ContainsFinding(ValidationResult validation, string code)
    {
        Assert.Contains(validation.Findings, f => f.Code == code);
    }
}