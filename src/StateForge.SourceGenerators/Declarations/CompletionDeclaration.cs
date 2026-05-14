using Microsoft.CodeAnalysis;

namespace StateForge.SourceGenerators.Declarations;

public sealed class CompletionDeclaration
{
    public CompletionDeclaration(string sourceStateKey, string sourceExpression, string targetStateKey,
        string targetExpression, Location? sourceLocation = null)
    {
        SourceStateKey = sourceStateKey;
        SourceExpression = sourceExpression;
        TargetStateKey = targetStateKey;
        TargetExpression = targetExpression;
        SourceLocation = sourceLocation;
    }

    public string SourceStateKey { get; }
    public string SourceExpression { get; }
    public string TargetStateKey { get; }
    public string TargetExpression { get; }
    public Location? SourceLocation { get; }
}
