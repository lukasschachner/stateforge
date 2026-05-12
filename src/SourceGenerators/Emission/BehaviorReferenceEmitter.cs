using StateMachineLibrary.SourceGenerators.Declarations;

namespace StateMachineLibrary.SourceGenerators.Emission;

public static class BehaviorReferenceEmitter
{
    public static string Emit(BehaviorReference reference)
    {
        var method = reference.Phase == BehaviorPhase.Exit ? "OnExit" :
            reference.Phase == BehaviorPhase.Entry ? "OnEntry" : "Execute";
        if (reference.UseAsyncBuilder) method += "Async";

        var display = reference.DisplayName is null
            ? string.Empty
            : ", " + ConditionReferenceEmitter.Literal(reference.DisplayName);
        return "." + method + "(" + reference.MemberName + display + ")";
    }
}