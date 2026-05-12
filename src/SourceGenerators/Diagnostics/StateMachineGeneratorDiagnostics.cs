using Microsoft.CodeAnalysis;

namespace StateMachineLibrary.SourceGenerators.Diagnostics;

public static class StateMachineGeneratorDiagnostics
{
    private const string Category = "StateMachineGenerator";

    public static readonly DiagnosticDescriptor DuplicateDeclaration = new(
        "SMG001", "Duplicate state machine declaration",
        "Duplicate {0} declaration '{1}'. Remove or rename one declaration.", Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor MissingReference = new(
        "SMG002", "Missing state machine declaration reference",
        "Transition references undeclared {0} '{1}'. Declare the {0} or fix the transition.", Category,
        DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor AmbiguousTransition = new(
        "SMG003", "Ambiguous state machine transition",
        "Multiple transitions from state '{0}' for event '{1}' are ambiguous. Keep only one transition or add distinct conditions supported by the runtime.",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor InvalidTerminalConfiguration = new(
        "SMG004", "Invalid terminal state configuration",
        "Terminal state '{0}' declares outgoing transitions. Remove those transitions or make the state non-terminal.",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor UnsupportedDslSyntax = new(
        "SMG005", "Unsupported state machine DSL syntax",
        "Unsupported compact DSL syntax '{0}'. Use only recognized declarative state machine calls.", Category,
        DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor InvalidMemberReference = new(
        "SMG006", "Invalid condition or behavior member reference",
        "Member reference '{0}' is missing or has an incompatible signature for {1}.", Category,
        DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor GeneratedNameConflict = new(
        "SMG007", "Generated member name conflict",
        "The generated member '{0}' conflicts with a user-authored member. Rename the member or declaration.", Category,
        DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor RuntimeValidationWarning = new(
        "SMG008", "Runtime validation warning", "Core validation may report '{0}' for the generated definition.",
        Category, DiagnosticSeverity.Warning, true);

    public static readonly DiagnosticDescriptor DuplicateRegionDeclaration = new(
        "SMG009", "Duplicate parallel region declaration",
        "Parallel composite '{0}' declares region '{1}' more than once. Keep one explicit region declaration and add members with Region declarations.",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor MissingRegionalInitial = new(
        "SMG010", "Missing parallel region initial state",
        "Parallel region '{1}' on composite '{0}' must declare exactly one initial state.", Category,
        DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor DuplicateRegionMembership = new(
        "SMG011", "Duplicate sibling region membership",
        "State '{0}' is assigned to multiple regions under parallel composite '{1}'. Assign it to only one sibling region.",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor UnknownRegionOwner = new(
        "SMG012", "Unknown parallel region owner",
        "Parallel region owner '{0}' is not a declared parallel composite state. Declare the owner state as a parallel composite or fix the region declaration.",
        Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor UnsupportedHistoryMode = new(
        "SMG013", "Unsupported history mode",
        "State '{0}' declares an unsupported history mode. Use None, Shallow, or Deep.", Category,
        DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor InvalidAdvancedRoleCombination = new(
        "SMG014", "Invalid advanced state declaration combination",
        "State '{0}' has an invalid advanced declaration: {1}.", Category, DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor InvalidRegionDeclaration = new(
        "SMG015", "Invalid parallel region declaration",
        "Parallel region '{1}' on composite '{0}' is invalid: {2}.", Category, DiagnosticSeverity.Error,
        true);
}