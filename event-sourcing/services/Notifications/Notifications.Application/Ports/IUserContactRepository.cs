using Notifications.Domain;

namespace Notifications.Application.Ports;

public interface IUserContactRepository
{
    Task<UserContact?> FindAsync(Guid userId, CancellationToken ct);

    Task SaveAsync(UserContact contact, CancellationToken ct);

    Task<int> CountAsync(CancellationToken ct);
}
