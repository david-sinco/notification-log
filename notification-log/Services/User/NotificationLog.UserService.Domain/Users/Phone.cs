using Domain.Shared.Exceptions;
using System.Text.RegularExpressions;

namespace NotificationLog.UserService.Domain.Users;

public sealed record Phone
{
    // Límites de la recomendación E.164: el número nacional significativo más el prefijo de
    // país nunca supera los 15 dígitos, y ningún plan de numeración asigna menos de 4 (los
    // números cortos tipo 112 no son direccionables desde el extranjero, así que no valen
    // como teléfono de contacto). El '+' inicial no cuenta como dígito, por eso MaxLength
    // es uno más que DigitsMaxLength. Se declaran públicas porque la capa de persistencia y
    // los validadores de aplicación necesitan exactamente el mismo número.
    public const int DigitsMinLength = 4;
    public const int DigitsMaxLength = 15;
    public const int MaxLength = DigitsMaxLength + 1;

    // Formato canónico E.164 ya normalizado: '+', un primer dígito distinto de cero (ningún
    // prefijo de país empieza por 0) y el resto de dígitos. No se valida contra la lista real
    // de prefijos asignados por la UIT: esa lista cambia y mantenerla en el dominio daría
    // falsos negativos; comprobar que el número es alcanzable es trabajo de la pasarela.
    private static readonly Regex Format =
        new(@"^\+[1-9]\d{3,14}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Separadores de presentación que se aceptan a la entrada y se descartan al normalizar:
    // son ruido tipográfico con el que la gente escribe los teléfonos (+34 600-123 456,
    // (+34) 600.123.456) y que no forma parte del número.
    private static readonly char[] Separators = [' ', '-', '.', '(', ')', '\u00A0'];

    public string Value { get; }

    private Phone(string value) => Value = value;

    public static Phone Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("El teléfono es obligatorio.");

        // El teléfono se guarda siempre en E.164 aunque el usuario lo haya escrito con
        // separadores o con el prefijo internacional en forma de '00': almacenar la forma
        // canónica es lo que permite tratar el Value Object como identidad comparable
        // (dos usuarios con el mismo número lo tienen escrito igual) y lo que esperan las
        // pasarelas de SMS, que rechazan cualquier otra representación.
        var normalized = Normalize(value);

        if (normalized.Length > MaxLength)
            throw new DomainException($"El teléfono no puede superar {DigitsMaxLength} dígitos.");

        if (!Format.IsMatch(normalized))
            throw new DomainException(
                $"El teléfono '{value}' no tiene un formato válido: debe incluir el prefijo " +
                $"internacional y entre {DigitsMinLength} y {DigitsMaxLength} dígitos " +
                "(ejemplo: +34600123456).");

        return new Phone(normalized);
    }

    private static string Normalize(string value)
    {
        var trimmed = value.Trim();
        var buffer = new System.Text.StringBuilder(trimmed.Length);

        foreach (var character in trimmed)
        {
            if (Separators.Contains(character))
                continue;

            // Cualquier carácter que no sea dígito ni el '+' inicial se rechaza en lugar de
            // ignorarse: una letra en mitad del número casi siempre significa que el dato
            // viene mal de origen, y limpiarlo en silencio guardaría un teléfono equivocado.
            if (!char.IsAsciiDigit(character) && !(character == '+' && buffer.Length == 0))
                throw new DomainException(
                    $"El teléfono '{value}' contiene caracteres no válidos.");

            buffer.Append(character);
        }

        var digits = buffer.ToString();

        // '00' es el prefijo de salida internacional más extendido (y el único que la ITU
        // recomienda), así que se traduce al '+' de E.164 en vez de rechazar el número.
        // Ojo con el orden: se comprueba antes de exigir el '+' para que "0034600123456"
        // entre como válido, pero un número nacional sin prefijo alguno no cuela, porque
        // sin país no se puede enrutar el SMS.
        if (digits.StartsWith("00", StringComparison.Ordinal))
            return string.Concat("+", digits.AsSpan(2));

        return digits;
    }

    /// <summary>
    /// Reconstruye el VO desde un valor ya persistido en un evento, saltándose la validación.
    /// Es interna y solo la usa el agregado al aplicar su historia: el valor se validó y
    /// normalizó el día que se emitió el evento, y volver a pasarlo por Create haría que
    /// endurecer el formato mañana rompiera la relectura de los usuarios de ayer.
    /// </summary>
    internal static Phone FromStorage(string value) => new(value);

    public override string ToString() => Value;
}
