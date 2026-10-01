using System.Text.RegularExpressions;
using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Visits.ValueObjects;

public sealed record VisitNote
{
    private static readonly Regex EmailPattern =
        new(@"[^@\s]+@[^@\s]+\.[^@\s]{2,}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex LinkPattern =
        new(@"(https?://|www\.)\S+|\b\S+\.(com|co|net|org|io|me|ly)\b",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex PhonePattern =
        new(@"(\+?\d[\s\-\.()]*){7,}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private VisitNote(string value) => Value = value;

    public static VisitNote Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("El mensaje de la visita no puede estar vacío.");

        var normalized = value.Trim();

        if (normalized.Length > VisitPolicy.MaxNoteLength)
            throw new DomainException(
                $"El mensaje de la visita no puede superar {VisitPolicy.MaxNoteLength} caracteres.");

        if (EmailPattern.IsMatch(normalized) || LinkPattern.IsMatch(normalized) || PhonePattern.IsMatch(normalized))
            throw new DomainException("El mensaje de la visita no puede contener teléfonos, correos ni enlaces.");

        return new VisitNote(normalized);
    }

    internal static VisitNote FromStorage(string value) => new(value);

    public override string ToString() => Value;
}
