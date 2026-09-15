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

    Task<bool> TryReserveEmailAsync(Email email, Guid userId, CancellationToken cancellationToken = default);

    Task ReleaseEmailAsync(Email email, Guid userId, CancellationToken cancellationToken = default);
}
