using System.Text.RegularExpressions;
using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Inquiries.ValueObjects;

public sealed record InquiryMessage
{
    public const int MaxLength = 1000;

    private const RegexOptions Options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    private static readonly Regex EmailPattern = new(@"[a-z0-9._%+-]+@[a-z0-9-]+(\.[a-z0-9-]+)*\.[a-z]{2,}", Options);
    private static readonly Regex LinkPattern = new(@"(https?://|www\.)\S+|\b[a-z0-9-]+\.(com|co|net|org|io|me|app)\b", Options);
    private static readonly Regex DigitSeparators = new(@"(?<=\d)[\s.\-()]+(?=\d)", Options);
    private static readonly Regex DigitRuns = new(@"\d+", Options);

    public string Text { get; }

    private InquiryMessage(string text) => Text = text;

    public static InquiryMessage Create(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("El mensaje no puede estar vacío.");

        var normalized = text.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException($"El mensaje no puede superar {MaxLength} caracteres.");

        if (EmailPattern.IsMatch(normalized) || LinkPattern.IsMatch(normalized) || ContainsPhone(normalized))
            throw new DomainException("Los mensajes no pueden incluir teléfonos, correos ni enlaces.");

        return new InquiryMessage(normalized);
    }

    internal static InquiryMessage FromStorage(string text) => new(text);

    public override string ToString() => Text;

    private static bool ContainsPhone(string text)
        => DigitRuns.Matches(DigitSeparators.Replace(text, string.Empty))
            .Any(run => run.Value.Length >= 12
                || (run.Value.Length == 10 && (run.Value.StartsWith('3') || run.Value.StartsWith("60"))));
}
