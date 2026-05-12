namespace StateMachineLibrary.Core.Validation;

/// <summary>Stable diagnostic codes emitted by active-state snapshot validation.</summary>
public static class ActiveStateSnapshotValidationCodes
{
    public const string DefinitionInvalid = "active_snapshot.definition_invalid";
    public const string DuplicateRegion = "active_snapshot.region.duplicate";
    public const string FingerprintMismatch = "active_snapshot.fingerprint_mismatch";
    public const string InvalidKind = "active_snapshot.kind.invalid";
    public const string InvalidPath = "active_snapshot.path.invalid";
    public const string InvalidRegionOwner = "active_snapshot.region.owner_invalid";
    public const string InvalidRegionState = "active_snapshot.region.state_invalid";
    public const string InvalidTerminalFlag = "active_snapshot.region.terminal_flag_invalid";
    public const string MissingActiveLeafState = "active_snapshot.active_leaf.missing";
    public const string MissingActivePath = "active_snapshot.path.missing";
    public const string MissingOwningCompositeState = "active_snapshot.owner.missing";
    public const string MissingRegion = "active_snapshot.region.missing";
    public const string RegionMetadataNotAllowed = "active_snapshot.region.not_allowed";
    public const string RegionNameMismatch = "active_snapshot.region.name_mismatch";
    public const string SequenceInvalid = "active_snapshot.sequence.invalid";
    public const string UnknownRegion = "active_snapshot.region.unknown";
    public const string UnknownState = "active_snapshot.state.unknown";
}
