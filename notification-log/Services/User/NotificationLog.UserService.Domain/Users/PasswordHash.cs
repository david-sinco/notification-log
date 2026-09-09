using Domain.Shared.Exceptions;

namespace NotificationLog.UserService.Domain.Users;

/// <summary>
/// La contraseña del usuario, ya hasheada. Existe como Value Object para que el resto del
/// dominio no pueda manejar contraseñas en claro ni por accidente: <see cref="Create"/> es el
/// único camino y exige el hasher, de modo que no hay forma de construir un usuario cuya
/// contraseña se haya guardado sin hashear.
/// </summary>
public sealed record PasswordHash
{
    // Política de contraseñas: es dominio, no infraestructura — la decide el negocio y debe dar
    // el mismo resultado en la API, en un comando de consola o en un test. El mínimo de 8 sigue
    // la recomendación del NIST SP 800-63B, que además desaconseja exigir mezcla de mayúsculas
    // y símbolos (empuja a "Password1!" y variantes predecibles), así que aquí solo se limita
    // la longitud. El máximo evita que alguien mande megabytes y convierta el hasher, que es
    // caro por diseño, en un ataque de denegación de servicio.
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;

    public string Value { get; }

    private PasswordHash(string value) => Value = value;

    public static PasswordHash Create(string plainPassword, IPasswordHasher hasher)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
            throw new DomainException("La contraseña es obligatoria.");

        // Sin Trim: los espacios al principio o al final son parte de la contraseña que el
        // usuario eligió, y recortarlos haría que una contraseña válida dejara de coincidir.
        if (plainPassword.Length < MinPasswordLength)
            throw new DomainException(
                $"La contraseña debe tener al menos {MinPasswordLength} caracteres.");

        if (plainPassword.Length > MaxPasswordLength)
            throw new DomainException(
                $"La contraseña no puede superar {MaxPasswordLength} caracteres.");

        return new PasswordHash(hasher.Hash(plainPassword));
    }

    /// <summary>
    /// Reconstruye el VO desde un hash ya persistido en un evento. No valida: la política de
    /// longitud se aplicó cuando se eligió la contraseña, y volver a comprobarla al releer la
    /// historia rompería la rehidratación el día que el mínimo suba.
    /// </summary>
    internal static PasswordHash FromStorage(string hash) => new(hash);

    public bool Matches(string plainPassword, IPasswordHasher hasher)
        => !string.IsNullOrEmpty(plainPassword) && hasher.Verify(plainPassword, Value);

    // El hash no se imprime nunca: un ToString descuidado acaba en un log.
    public override string ToString() => "***";
}
