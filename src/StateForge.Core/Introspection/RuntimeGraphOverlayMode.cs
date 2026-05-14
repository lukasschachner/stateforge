namespace StateForge.Core.Introspection;

/// <summary>Controls whether runtime graph export attaches active-state overlay metadata.</summary>
public enum RuntimeGraphOverlayMode
{
    /// <summary>Do not include runtime active-state overlay metadata.</summary>
    None = 0,

    /// <summary>Include active-state overlay metadata captured from the runtime instance.</summary>
    ActiveState = 1
}
