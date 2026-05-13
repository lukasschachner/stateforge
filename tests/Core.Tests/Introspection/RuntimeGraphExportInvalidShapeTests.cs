using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Tests.Introspection;

public sealed class RuntimeGraphExportInvalidShapeTests
{
    [Fact]
    public void Runtime_export_fails_when_active_leaf_is_not_declared()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateFlatDefinition().CreateRuntime(RuntimeGraphState.Unknown);

        var exception = Assert.Throws<ActiveStateSnapshotValidationException<RuntimeGraphState>>(() => runtime.ExportGraph());

        Assert.Contains(exception.ValidationResult.Diagnostics,
            diagnostic => diagnostic.Code == ActiveStateSnapshotValidationCodes.UnknownState);
    }

    [Fact]
    public async Task External_runtime_export_fails_when_accessor_returns_unknown_state()
    {
        var runtime = RuntimeGraphExportTestDomain.CreateFlatDefinition()
            .CreateRuntime(StateMachineLibrary.Core.Execution.StateAccessor.Create(
                () => RuntimeGraphState.Unknown,
                _ => { }));

        var exception = await Assert.ThrowsAsync<ActiveStateSnapshotValidationException<RuntimeGraphState>>(async () =>
            await runtime.ExportGraphAsync());

        Assert.Contains(exception.ValidationResult.Diagnostics,
            diagnostic => diagnostic.Code == ActiveStateSnapshotValidationCodes.UnknownState);
    }
}
