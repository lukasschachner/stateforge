namespace StateForge.Core.Validation;

public static class ParallelValidationCodes
{
    public const string ZeroRegions = "PARALLEL001";
    public const string MissingInitial = "PARALLEL002";
    public const string DuplicateRegionName = "PARALLEL003";
    public const string BlankRegionName = "PARALLEL004";
    public const string InvalidMembership = "PARALLEL005";
    public const string IllegalBoundaryTransition = "PARALLEL006";
    public const string AmbiguousEvent = "PARALLEL007";
    public const string UnreachableRegionalState = "PARALLEL008";
    public const string DirectHistoryUnsupported = "PARALLEL009";
    public const string InvalidHistoryConfiguration = "PARALLEL010";
    public const string MissingFallback = "PARALLEL011";
    public const string InvalidFallback = "PARALLEL012";
    public const string SuppliedHistoryInvalid = "PARALLEL013";
    public const string InvalidRestorePath = "PARALLEL014";
    public const string DuplicateRegionHistory = "PARALLEL015";
    public const string UnknownRegionHistory = "PARALLEL016";
    public const string UnknownStateHistory = "PARALLEL017";
}