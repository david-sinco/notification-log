using System.Net;
using System.Text.RegularExpressions;

namespace NotificationLog.Web.Components.Templates;

public static partial class TemplateRendering
{
    public const string RecipientPrefix = "recipient.";

    [GeneratedRegex(@"\{\{\s*([\w.]+)\s*\}\}")]
    private static partial Regex Placeholder();

    [GeneratedRegex(@"[^\n\r\x20-\x7EéèùìòÇØøÅåÑñÜüÄäÖöàÉßÆæ¿¡£¥§€]")]
    private static partial Regex OutsideGsm();

    [GeneratedRegex(@"<(script|style|head)\b.*?</\1\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex HiddenBlock();

    [GeneratedRegex(@"<br\s*/?>|</(p|div|tr|h[1-6]|li)\s*>", RegexOptions.IgnoreCase)]
    private static partial Regex LineBreakTag();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex Tag();

    [GeneratedRegex(@"[ \t]*\n\s*\n\s*")]
    private static partial Regex BlankLines();

    public static bool IsHtml(string body) => body.TrimStart().StartsWith('<');

    public static string Render(string text, IReadOnlyDictionary<string, string> values) =>
        Placeholder().Replace(text, match => Value(values, match.Groups[1].Value) ?? match.Value);

    public static string RenderHtml(string html, IReadOnlyDictionary<string, string> values) =>
        Placeholder().Replace(html, match => Value(values, match.Groups[1].Value) is { } value ? WebUtility.HtmlEncode(value) : match.Value);

    public static string ToPlainText(string body)
    {
        if (!IsHtml(body))
            return body;

        var text = HiddenBlock().Replace(body, "");
        text = LineBreakTag().Replace(text, "\n");
        text = WebUtility.HtmlDecode(Tag().Replace(text, ""));
        return BlankLines().Replace(text, "\n\n").Trim();
    }

    public static IReadOnlyList<PreviewSegment> Segments(string text, IReadOnlyDictionary<string, string> values)
    {
        var segments = new List<PreviewSegment>();
        var last = 0;

        foreach (Match match in Placeholder().Matches(text))
        {
            if (match.Index > last)
                segments.Add(new PreviewSegment(text[last..match.Index], null, false));

            var key = match.Groups[1].Value;
            var value = Value(values, key);
            segments.Add(new PreviewSegment(value ?? match.Value, key, value is null));
            last = match.Index + match.Length;
        }

        if (last < text.Length)
            segments.Add(new PreviewSegment(text[last..], null, false));

        return segments;
    }

    public static IReadOnlyList<string> Keys(string text) =>
        Placeholder().Matches(text).Select(match => match.Groups[1].Value).Distinct().ToList();

    public static IReadOnlyList<string> EventKeys(string text) =>
        Keys(text).Where(key => !key.StartsWith(RecipientPrefix, StringComparison.Ordinal)).ToList();

    public static IReadOnlyList<string> MissingKeys(string text, IReadOnlyDictionary<string, string> values) =>
        Keys(text).Where(key => Value(values, key) is null).ToList();

    public static (int Length, int Segments, bool Gsm) Sms(string text)
    {
        var gsm = !OutsideGsm().IsMatch(text);
        var single = gsm ? 160 : 70;
        var perSegment = text.Length > single ? (gsm ? 153 : 67) : single;
        return (text.Length, Math.Max(1, (int)Math.Ceiling(text.Length / (double)perSegment)), gsm);
    }

    private static string? Value(IReadOnlyDictionary<string, string> values, string key) =>
        values.TryGetValue(key, out var value) && value.Length > 0 ? value : null;
}
