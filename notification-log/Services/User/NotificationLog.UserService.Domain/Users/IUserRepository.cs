namespace NotificationLog.UserService.Domain.Users;

/// <summary>
/// Puerto de persistencia del agregado. La firma es deliberadamente pobre — cargar y añadir —
/// porque con event sourcing no hay "update": guardar es siempre añadir los eventos nuevos al
/// final del flujo.
/// </summary>
public interface IUserRepository
{
    /// <summary>Rehidrata el usuario reproduciendo su flujo. Null si el flujo no existe.</summary>
    Task<User?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade al flujo los <see cref="Domain.Shared.Common.AggregateRoot.DomainEvents"/>
    /// pendientes y los limpia. El control de concurrencia es cosa de la implementación: la
    /// versión del flujo no vive en el dominio, la lleva el store (en Marten, cargando el flujo
    /// para escritura y dejando que falle al guardar si alguien se adelantó).
    /// </summary>
    Task AppendAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca el identificador de flujo a partir del correo, que es lo que hace falta para el
    /// login y para rechazar altas duplicadas.
    /// </summary>
    /// <remarks>
    /// "No puede haber dos usuarios con el mismo correo" es una invariante <em>del conjunto</em>,
    /// no de un agregado: ninguna instancia de User puede verificarla mirándose a sí misma. Se
    /// resuelve fuera, con una proyección de credenciales con índice único sobre el correo; si
    /// esa proyección se construye en línea con la escritura, la unicidad es fuerte, y si es
    /// asíncrona hay que asumir una ventana en la que dos altas simultáneas pasan el chequeo.
    /// </remarks>
    Task<Guid?> FindIdByEmailAsync(Email email, CancellationToken cancellationToken = default);
}
