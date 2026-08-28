using NotificationLog.NotificationService.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NotificationLog.NotificationService.Domain.Templates;

public sealed record TemplateName
{
    public const int MaxLength = 100;

    private static readonly Regex Format =
        new(@"^[a-z0-9]+(_[a-z0-9]+)*$", RegexOptions.Compiled);

    public string Value { get; }

    private TemplateName(string value) => Value = value;

    public static TemplateName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("El nombre de la plantilla es obligatorio.");

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            throw new DomainException($"El nombre no puede superar {MaxLength} caracteres.");

        if (!Format.IsMatch(normalized))
            throw new DomainException(
                $"El nombre '{value}' solo admite minúsculas, números y guion bajo.");

        return new TemplateName(normalized);
    }

    public override string ToString() => Value;
}