namespace StateForge.SourceGenerators.Tests;

internal static class GeneratedSourceAssertions
{
    public static void ContainsInOrder(string source, params string[] snippets)
    {
        var index = 0;
        foreach (var snippet in snippets)
        {
            var next = source.IndexOf(snippet, index, StringComparison.Ordinal);
            Assert.True(next >= 0, $"Expected generated source to contain '{snippet}' after index {index}.\n{source}");
            index = next + snippet.Length;
        }
    }

    public static void DoesNotContainEnvironmentSpecificContent(string source)
    {
        Assert.DoesNotContain(Environment.CurrentDirectory, source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.GetTempPath(), source, StringComparison.OrdinalIgnoreCase);
    }
}
