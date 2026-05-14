namespace StateForge.SourceGenerators.Tests;

internal static class ValidationDiagnosticTestSources
{
    public const string BasicEnums = """
                                     using StateForge.SourceGeneration;
                                     public enum S { A, B, C, Done }
                                     public enum E { Go, Next, Finish }
                                     """;

    public static string Machine(string body) => BasicEnums + "\n" + body;
}
