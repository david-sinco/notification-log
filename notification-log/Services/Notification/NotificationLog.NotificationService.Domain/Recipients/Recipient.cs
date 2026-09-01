using NotificationLog.NotificationService.Domain.Common;
using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Domain.Recipients;

public sealed class Recipient : AggregateRoot
{
    public const int NameMaxLength = 200;
    public const int AttributeKeyMaxLength = 100;
    public const int EmailMaxLength = 320;
    public const int PhoneMaxLength = 20;
    public const int LocaleMaxLength = 20;
    public const int TimeZoneMaxLength = 50;

    private readonly Dictionary<string, string> _attributes = new();

    public string Name { get; private set; } = default!;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string Locale { get; private set; } = "es-CO";
    public string TimeZone { get; private set; } = "America/Bogota";
    public bool IsActive { get; private set; }

    public IReadOnlyDictionary<string, string> Attributes => _attributes;

    private Recipient() { }   // EF Core

    public static Recipient Create(Guid userId, string name, string? email, string? phone)
    {
        if (userId == Guid.Empty)
            throw new DomainException("El identificador del destinatario es obligatorio.");

        return new Recipient
        {
            Id = userId,
            Name = NormalizeName(name),
            Email = NormalizeEmail(email),
            Phone = NormalizePhone(phone),
            IsActive = true
        };
    }

    // ---------- Perfil ----------

    public void ChangeName(string name) => Name = NormalizeName(name);

    public void ChangeLocalization(string? locale, string? timeZone)
    {
        if (!string.IsNullOrWhiteSpace(locale)) Locale = locale.Trim();
        if (!string.IsNullOrWhiteSpace(timeZone)) TimeZone = timeZone.Trim();
    }

    // ---------- Canales ----------

    public void ChangeEmail(string? email) => Email = NormalizeEmail(email);

    public void ChangePhone(string? phone) => Phone = NormalizePhone(phone);

    public string? AddressFor(NotificationChannel channel) => channel switch
    {
        NotificationChannel.Email => Email,
        NotificationChannel.Sms or NotificationChannel.WhatsApp => Phone,
        _ => null
    };

    public bool CanReceive(NotificationChannel channel)
        => IsActive && !string.IsNullOrWhiteSpace(AddressFor(channel));

    // ---------- Atributos ----------

    public void SetAttribute(string key, string? value)
    {
        var normalizedKey = NormalizeKey(key);

        if (string.IsNullOrWhiteSpace(value))
            _attributes.Remove(normalizedKey);
        else
            _attributes[normalizedKey] = value.Trim();
    }

    public void SetAttributes(IReadOnlyDictionary<string, string?> attributes)
    {
        foreach (var (key, value) in attributes)
            SetAttribute(key, value);
    }

    public void ReplaceAttributes(IReadOnlyDictionary<string, string?> attributes)
    {
        _attributes.Clear();
        SetAttributes(attributes);
    }

    public void RemoveAttribute(string key) => _attributes.Remove(NormalizeKey(key));

    public string? GetAttribute(string key)
        => _attributes.GetValueOrDefault(NormalizeKey(key));

    // ---------- Estado ----------

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    // ---------- Normalización ----------

    private static string NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException("La clave del atributo es obligatoria.");

        var normalized = key.Trim().ToLowerInvariant();

        return normalized.Length > AttributeKeyMaxLength
            ? normalized[..AttributeKeyMaxLength]
            : normalized;
    }

    private static string? NormalizeEmail(string? email)
        => string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    private static string? NormalizePhone(string? phone)
        => string.IsNullOrWhiteSpace(phone) ? null : phone.Replace(" ", "").Replace("-", "");

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del destinatario es obligatorio.");

        var trimmed = name.Trim();
        return trimmed.Length > NameMaxLength ? trimmed[..NameMaxLength] : trimmed;
    }
}