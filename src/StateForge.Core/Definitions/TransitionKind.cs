namespace StateForge.Core.Definitions;

/// <summary>Specifies how a transition affects state and lifecycle behavior.</summary>
public enum TransitionKind
{
    External,
    Self,
    Internal
}