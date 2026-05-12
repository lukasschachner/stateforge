using System.Collections;
using System.Globalization;
using System.Text;
using StateMachineLibrary.Core.Definitions;
using StateMachineLibrary.Core.Introspection;

namespace StateMachineLibrary.Visualization.PlantUML.Rendering;

internal static class PlantUmlMetadataFormatter
{
    public static string Format(MetadataCollection metadata)
    {
        if (metadata.Count == 0) return string.Empty;

        return string.Join(", ",
            metadata.OrderBy(m => m.Key, StringComparer.Ordinal)
                .Select(m => $"{m.Key}={FormatValue(m.Value)}"));
    }

    public static string FormatActions(IEnumerable<GraphActionSummary> actions)
    {
        var parts = actions
            .OrderBy(a => a.Kind)
            .ThenBy(a => a.Order)
            .ThenBy(a => a.DisplayName, StringComparer.Ordinal)
            .Select(a => $"{a.Kind}[{a.Order}]={a.DisplayName}")
            .ToArray();
        return parts.Length == 0 ? string.Empty : string.Join(", ", parts);
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => "null",
            string text => text.Replace("\r\n", "\\n", StringComparison.Ordinal)
                .Replace("\n", "\\n", StringComparison.Ordinal),
            bool boolean => boolean ? "true" : "false",
            DateTime dateTime => dateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToUniversalTime()
                .ToString("O", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
            IDictionary dictionary => FormatDictionary(dictionary),
            IEnumerable sequence when value is not string => FormatSequence(sequence),
            _ => value.GetType().FullName ?? value.ToString() ?? string.Empty
        };
    }

    private static string FormatDictionary(IDictionary dictionary)
    {
        var entries = new List<(string Key, object? Value)>();
        foreach (DictionaryEntry entry in dictionary) entries.Add((entry.Key?.ToString() ?? "<null>", entry.Value));

        return "{" + string.Join(", ",
            entries.OrderBy(e => e.Key, StringComparer.Ordinal).Select(e => $"{e.Key}:{FormatValue(e.Value)}")) + "}";
    }

    private static string FormatSequence(IEnumerable sequence)
    {
        var values = new StringBuilder("[");
        var first = true;
        foreach (var item in sequence)
        {
            if (!first) values.Append(", ");

            values.Append(FormatValue(item));
            first = false;
        }

        values.Append(']');
        return values.ToString();
    }
}