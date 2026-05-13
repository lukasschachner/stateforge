namespace StateMachineLibrary.Core.Introspection;

/// <summary>Options for side-effect-free graph export from a running state machine.</summary>
public sealed class RuntimeGraphExportOptions
{
    /// <summary>Gets or sets the runtime overlay metadata mode. Defaults to active-state overlay export.</summary>
    public RuntimeGraphOverlayMode OverlayMode { get; set; } = RuntimeGraphOverlayMode.ActiveState;

    /// <summary>
    /// Gets or sets whether active-state shape data is validated before overlay metadata is returned.
    /// Runtime active-state overlays require validation in this version.
    /// </summary>
    public bool ValidateActiveShape { get; set; } = true;

    internal void Validate()
    {
        if (!Enum.IsDefined(OverlayMode))
            throw new ArgumentOutOfRangeException(nameof(OverlayMode), OverlayMode,
                "Runtime graph overlay mode is not supported.");

        if (OverlayMode == RuntimeGraphOverlayMode.ActiveState && !ValidateActiveShape)
            throw new ArgumentException(
                "Runtime graph active-state overlays require active-shape validation.",
                nameof(ValidateActiveShape));
    }
}
