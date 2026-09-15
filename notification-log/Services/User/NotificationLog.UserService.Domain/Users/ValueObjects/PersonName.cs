using Domain.Shared.Exceptions;

namespace NotificationLog.UserService.Domain.Users;

/// <summary>
/// El nombre con el que se dirige uno al usuario. Es un Value Object por coherencia con
/// <see cref="Email"/> y <see cref="Phone"/> —los tres son datos de contacto con reglas de
/// formato propias— y para que esas reglas dejen de vivir sueltas dentro del agregado.
/// </summary>
public sealed record PersonName
{
    // El límite no busca validar nada: es el ancho con el que se guarda y se pinta. Se declara
    // público porque la capa de persistencia y los validadores de aplicación necesitan el mismo
    // número, igual que en Email y Phone.
    public const int MaxLength = 120;

    public string Value { get; }

    private PersonName(string value) => Value = value;

    public static PersonName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("El nombre del usuario es obligatorio.");

        var normalized = Normalize(value);

        // Puede quedar vacío aunque la entrada no lo pareciera: un valor formado solo por
        // caracteres de control se queda en nada al normalizar.
        if (normalized.Length == 0)
            throw new DomainException($"El nombre '{value}' no contiene ningún carácter válido.");

        // Se corta en vez de lanzar: un nombre larguísimo es un dato feo, no una violación de
        // negocio que deba tumbar el alta. El TrimEnd evita que el corte deje un espacio final.
        return new PersonName(
            normalized.Length > MaxLength ? normalized[..MaxLength].TrimEnd() : normalized);
    }

    /// <summary>
    /// Reconstruye el VO desde un valor ya persistido en un evento, saltándose la validación.
    /// Es interna y solo la usa el agregado al aplicar su historia: el valor se validó y
    /// normalizó el día que se emitió el evento, y volver a pasarlo por Create haría que
    /// endurecer el formato mañana rompiera la relectura de los usuarios de ayer.
    /// </summary>
    internal static PersonName FromStorage(string value) => new(value);

    private static string Normalize(string value)
    {
        var buffer = new System.Text.StringBuilder(value.Length);
        var pendingSeparator = false;

        foreach (var character in value)
        {
            // Los caracteres de control se descartan en lugar de rechazar el nombre entero.
            // No es cosmético: este nombre acaba renderizado en plantillas y, en el canal de
            // correo, dentro de una cabecera; un salto de línea ahí es inyección de cabeceras,
            // no un nombre mal escrito. Filtrarlo en el dominio evita depender de que cada
            // canal de salida se acuerde de hacerlo.
            if (char.IsControl(character))
                continue;

            // Los espacios interiores se colapsan a uno solo — "David   Gonzalez" y
            // "David Gonzalez" son el mismo nombre, y guardarlos igual es lo que permite
            // comparar el Value Object y detectar que un cambio no cambia nada. El separador
            // se anota y solo se escribe cuando llega el siguiente carácter real, lo que de
            // paso recorta principio y final sin un Trim aparte.
            if (char.IsWhiteSpace(character))
            {
                pendingSeparator = buffer.Length > 0;
                continue;
            }

            if (pendingSeparator)
            {
                buffer.Append(' ');
                pendingSeparator = false;
            }

            buffer.Append(character);
        }

        return buffer.ToString();
    }

    public override string ToString() => Value;
}
