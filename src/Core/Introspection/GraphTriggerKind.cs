namespace StateMachineLibrary.Core.Introspection;

/// <summary>Renderer-neutral trigger classification for graph edges.</summary>
public enum GraphTriggerKind
{
    /// <summary>The edge represents an event-triggered transition.</summary>
    Event = 0,

    /// <summary>The edge represents a completion-triggered transition.</summary>
    Completion = 1
}
