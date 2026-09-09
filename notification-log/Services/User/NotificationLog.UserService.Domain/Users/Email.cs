using Domain.Shared.Exceptions;
using System.Text.RegularExpressions;

namespace NotificationLog.UserService.Domain.Users;

public sealed record Email
{
    // Límites de RFC 5321: 64 caracteres para la parte local, 255 para el dominio y
    // 320 en total contando la arroba. Se declaran como constantes públicas porque la
    // capa de persistencia y los validadores de aplicación necesitan el mismo número.
    public const int MaxLength = 320;
    public const int LocalPartMaxLength = 64;
    public const int DomainMaxLength = 255;

    // No se valida el ABNF completo del RFC 5322 (admite comentarios, comillas y literales
    // IP que ninguna pasarela real acepta): se valida el subconjunto que sí se puede enviar.
    // Parte local: átomos de caracteres permitidos separados por puntos simples, de modo que
    // el punto nunca queda al principio, al final ni duplicado.
    // Dominio: etiquetas alfanuméricas que pueden llevar guiones internos, seguidas de un TLD
    // de al menos dos letras; se exige al menos un punto para rechazar dominios locales.
    private static readonly Regex Format =
       new(@"^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@([a-z0-9]([a-z0-9-]*[a-z0-9])?\.)+[a-z]{2,}$",
           RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("El correo es obligatorio.");

        // El correo se normaliza a minúsculas aunque el RFC considere la parte local
        // sensible a mayúsculas: ningún proveedor real distingue, y guardarlo normalizado
        // es lo que permite tratar el Value Object como identidad comparable y detectar
        // duplicados sin comparaciones especiales en base de datos.
        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            throw new DomainException($"El correo no puede superar {MaxLength} caracteres.");

        if (!Format.IsMatch(normalized))
            throw new DomainException(
                $"El correo '{value}' no tiene un formato válido (ejemplo: nombre@dominio.com).");

        // Las longitudes por tramo se comprueban aparte del patrón: meterlas en el regex
        // obligaría a lookaheads que lo vuelven ilegible y propensos a retroceso excesivo.
        var separator = normalized.IndexOf('@');

        if (separator > LocalPartMaxLength)
            throw new DomainException(
                $"La parte local del correo no puede superar {LocalPartMaxLength} caracteres.");

        if (normalized.Length - separator - 1 > DomainMaxLength)
            throw new DomainException(
                $"El dominio del correo no puede superar {DomainMaxLength} caracteres.");

        return new Email(normalized);
    }

    /// <summary>
    /// Reconstruye el VO desde un valor ya persistido en un evento, saltándose la validación.
    /// Es interna y solo la usa el agregado al aplicar su historia: el valor se validó y
    /// normalizó el día que se emitió el evento, y volver a pasarlo por Create haría que
    /// endurecer el formato mañana rompiera la relectura de los usuarios de ayer.
    /// </summary>
    internal static Email FromStorage(string value) => new(value);

    public override string ToString() => Value;
}
