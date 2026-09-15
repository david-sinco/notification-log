using Application.Shared.Abstractions;
using Application.Shared.Common;
using NotificationLog.UserService.Domain.Users;

namespace NotificationLog.UserService.Application.Users.Commands.SetUserStatus;

public sealed class SetUserStatusHandler
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;

    public SetUserStatusHandler(IUserRepository users, IUnitOfWork uow)
        => (_users, _uow) = (users, uow);

    public async Task HandleAsync(SetUserStatusCommand cmd, CancellationToken ct)
    {
        var user = await _users.LoadAsync(cmd.UserId, ct)
            ?? throw new NotFoundException(nameof(User), cmd.UserId);

        if (cmd.IsActive) user.Reactivate();
        else user.Deactivate(cmd.Reason);

        await _users.AppendAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
