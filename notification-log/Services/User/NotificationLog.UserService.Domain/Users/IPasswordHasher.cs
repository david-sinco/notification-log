namespace NotificationLog.UserService.Domain.Users;

/// <summary>
/// Puerto para el algoritmo de hash de contraseñas. Vive en el dominio porque el agregado lo
/// necesita para decidir (validar un login es una regla de negocio), pero se declara como
/// interfaz porque <em>qué</em> algoritmo se usa no lo es: Argon2 o bcrypt, con qué coste y
/// con qué política de rehash es una decisión de infraestructura que cambia con el hardware.
/// </summary>
/// <remarks>
/// El agregado lo recibe como parámetro del método que lo necesita, no como campo: un agregado
/// event-sourced se rehidrata desde eventos y no puede depender de que alguien le inyecte
/// colaboradores en el constructor.
/// </remarks>
public interface IPasswordHasher
{
    string Hash(string plainPassword);

    /// <summary>
    /// Debe comparar en tiempo constante y devolver false ante un hash con formato desconocido
    /// en lugar de lanzar: un hash viejo e ilegible es un login fallido, no un error 500.
    /// </summary>
    bool Verify(string plainPassword, string hash);
}
