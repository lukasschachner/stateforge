namespace StateMachineLibrary.Core.Execution;

/// <summary>Categories returned by every transition attempt.</summary>
public enum TransitionOutcomeCategory
{
    Success,
    Denied,
    NotPermitted,
    ValidationFailure,
    Cancelled,
    BehaviorFailure
}