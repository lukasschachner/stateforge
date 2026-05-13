namespace StateMachineLibrary.SourceGenerators.Declarations;

public enum GeneratedHelperAvailability
{
    Generated,
    Skipped
}

public enum GeneratedHelperSkippedReason
{
    None,
    GeneratedNameConflict,
    DuplicateEventIdentity,
    UnsupportedEventShape,
    AmbiguousHelperCandidate,
    RequiredMemberConflict
}

public sealed class GeneratedHelperModel
{
    public GeneratedHelperModel(string eventKey, string helperName, GeneratedHelperAvailability availability,
        GeneratedHelperSkippedReason skippedReason)
    {
        EventKey = eventKey;
        HelperName = helperName;
        Availability = availability;
        SkippedReason = skippedReason;
    }

    public string EventKey { get; }
    public string HelperName { get; }
    public GeneratedHelperAvailability Availability { get; }
    public GeneratedHelperSkippedReason SkippedReason { get; }
}
