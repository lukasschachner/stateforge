namespace StateForge.Visualization.Tests.TestSupport;

internal static class TextNormalization
{
    public static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
    }

    public static string NormalizeForSnapshot(string text)
    {
        return string.Join("\n", NormalizeLineEndings(text)
                .Split('\n')
                .Select(line => line.TrimEnd()))
            .TrimEnd() + "\n";
    }
}