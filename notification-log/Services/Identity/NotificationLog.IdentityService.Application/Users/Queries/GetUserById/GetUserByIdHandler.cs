using System.Security.Claims;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using NotificationLog.IdentityService.Application.Users.Dtos;
using NotificationLog.IdentityService.Domain.Users;

namespace NotificationLog.IdentityService.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdHandler
{
    private readonly IUserRepository _users;
    private readonly TimeProvider _time;

    public GetUserByIdHandler(IUserRepository users, TimeProvider time) => (_users, _time) = (users, time);

    public async Task<UserDto> HandleAsync(GetUserByIdQuery query, ClaimsPrincipal principal, CancellationToken ct)
    {
        if (principal.GetUserId() != query.Id && !principal.IsAdministrador())
            throw new ForbiddenException("Solo puedes consultar tu propio perfil.");

        var user = await _users.FindByIdAsync(query.Id, ct)
            ?? throw new NotFoundException("Usuario", query.Id);

        return UserDto.From(user, _time.GetUtcNow());
    }
}
