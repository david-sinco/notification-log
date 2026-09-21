using NotificationLog.IdentityService.Domain.Users.ValueObjects;

namespace NotificationLog.IdentityService.Domain.Users;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(Guid id, CancellationToken ct);

    Task<User?> FindByLoginAsync(LoginIdentifier login, CancellationToken ct);

    Task<IReadOnlyList<User>> SearchAsync(string? term, int skip, int take, CancellationToken ct);

    Task AddAsync(User user, CancellationToken ct);

    Task UpdateAsync(User user, CancellationToken ct);
}
