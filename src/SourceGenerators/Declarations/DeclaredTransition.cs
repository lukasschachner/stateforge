using Microsoft.CodeAnalysis;

namespace StateMachineLibrary.SourceGenerators.Declarations;

public enum DeclaredTransitionKind
{
    External,
    Self,
    Internal
}

public sealed class DeclaredTransition
{
    public DeclaredTransition(string transitionId, string sourceStateKey, string eventKey, string targetStateKey,
        string targetExpression, DeclaredTransitionKind transitionKind, Location? sourceLocation = null)
    {
        TransitionId = transitionId;
        SourceStateKey = sourceStateKey;
        EventKey = eventKey;
        TargetStateKey = targetStateKey;
        TargetExpression = targetExpression;
        TransitionKind = transitionKind;
        SourceLocation = sourceLocation;
    }

    public string TransitionId { get; }
    public string SourceStateKey { get; }
    public string EventKey { get; }
    public string TargetStateKey { get; }
    public string TargetExpression { get; }
    public DeclaredTransitionKind TransitionKind { get; }
    public List<ConditionReference> Conditions { get; } = new();
    public List<BehaviorReference> Behaviors { get; } = new();
    public List<MetadataEntry> Metadata { get; } = new();
    public Location? SourceLocation { get; }
}