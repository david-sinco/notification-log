namespace NotificationLog.IdentityService.Application.Abstractions;

public interface ISessionRevoker
{
    Task RevokeAllAsync(Guid userId, CancellationToken ct);
}
