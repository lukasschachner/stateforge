using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Execution;
using StateMachineLibrary.Core.Tests.History;
using StateMachineLibrary.Core.Validation;

namespace StateMachineLibrary.Core.Tests.Validation;

public sealed class ParallelHistoryValidationTests
{
    [Fact]
    public void Supplied_snapshot_rejects_unknown_composite_state()
    {
        var definition = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Shallow);
        var snapshot =
            new ParallelHistorySnapshot<ParallelHistoryState>((ParallelHistoryState)999, HistoryMode.Shallow, [], false,
                0);

        Assert.Contains(definition.ValidateParallelHistorySnapshot(snapshot).Errors,
            f => f.Code == ParallelValidationCodes.SuppliedHistoryInvalid);
    }

    [Fact]
    public void Supplied_snapshot_rejects_unknown_region_duplicate_region_unknown_state_and_invalid_path()
    {
        var definition = ParallelHistoryTestDomain.CreateTwoRegionDefinition(HistoryMode.Shallow);
        var validPath = definition.GetActiveStatePath(ParallelHistoryState.Packing);
        var invalidPath = new ActiveStatePath<ParallelHistoryState>([
            ParallelHistoryState.Operational, ParallelHistoryState.WaitingForPayment, ParallelHistoryState.Packing
        ]);
        var entries = new[]
        {
            new ParallelRegionHistoryEntry<ParallelHistoryState>("unknown", "Unknown", 0, ParallelHistoryState.Packing,
                validPath, 1),
            new ParallelRegionHistoryEntry<ParallelHistoryState>("region-000", "Fulfillment", 0,
                ParallelHistoryState.Packing, validPath, 2),
            new ParallelRegionHistoryEntry<ParallelHistoryState>("region-000", "Fulfillment", 0,
                ParallelHistoryState.Packing, validPath, 3),
            new ParallelRegionHistoryEntry<ParallelHistoryState>("region-001", "Billing", 1,
                (ParallelHistoryState)998,
                new ActiveStatePath<ParallelHistoryState>([
                    ParallelHistoryState.Operational, (ParallelHistoryState)998
                ]), 4),
            new ParallelRegionHistoryEntry<ParallelHistoryState>("region-001", "Billing", 1,
                ParallelHistoryState.Packing, invalidPath, 5)
        };
        var snapshot = new ParallelHistorySnapshot<ParallelHistoryState>(ParallelHistoryState.Operational,
            HistoryMode.Shallow, entries, false, 5);

        var errors = definition.ValidateParallelHistorySnapshot(snapshot).Errors;

        Assert.Contains(errors, f => f.Code == ParallelValidationCodes.UnknownRegionHistory);
        Assert.Contains(errors, f => f.Code == ParallelValidationCodes.DuplicateRegionHistory);
        Assert.Contains(errors, f => f.Code == ParallelValidationCodes.UnknownStateHistory);
        Assert.Contains(errors, f => f.Code == ParallelValidationCodes.InvalidRestorePath);
    }
}