namespace StateForge.Core.Definitions;

/// <summary>Well-known metadata keys understood by Core and optional integrations.</summary>
public static class StateMachineMetadataKeys
{
    /// <summary>Optional stable logical name for a state machine definition.</summary>
    public const string Name = "state_machine.name";

    /// <summary>Optional stable fingerprint used to validate externally supplied active-state snapshots.</summary>
    public const string DefinitionFingerprint = "state_machine.definition_fingerprint";
}