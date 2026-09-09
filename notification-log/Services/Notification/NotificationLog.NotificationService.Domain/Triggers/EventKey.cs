using Domain.Shared.Exceptions;
using System.Text.RegularExpressions;

namespace NotificationLog.NotificationService.Domain.Triggers;

public sealed record EventKey
{
    public const int MaxLength = 200;

    private static readonly Regex Format =
        new(@"^[a-z0-9]+(\.[a-z0-9]+)+$", RegexOptions.Compiled);

    public string Value { get; }

    private EventKey(string value) => Value = value;

    public static EventKey Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("El event key es obligatorio.");

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            throw new DomainException($"El event key no puede superar {MaxLength} caracteres.");

        if (!Format.IsMatch(normalized))
            throw new DomainException(
                $"El event key '{value}' debe tener el formato dominio.accion.");

        return new EventKey(normalized);
    }

    public override string ToString() => Value;
}
