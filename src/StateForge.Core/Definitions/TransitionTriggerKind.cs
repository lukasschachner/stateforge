namespace StateForge.Core.Definitions;

/// <summary>Classifies the trigger category that selected a transition.</summary>
public enum TransitionTriggerKind
{
    /// <summary>The transition is selected by matching a user-supplied event.</summary>
    Event = 0,

    /// <summary>The transition is selected when a completion-capable scope completes.</summary>
    Completion = 1
}
