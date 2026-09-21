using NotificationLog.IdentityService.Application.Common;
using NotificationLog.IdentityService.Application.Users.Dtos;
using NotificationLog.IdentityService.Domain.Users;

namespace NotificationLog.IdentityService.Application.Users.Queries.SearchUsers;

public sealed class SearchUsersHandler
{
    private readonly IUserRepository _users;
    private readonly TimeProvider _time;

    public SearchUsersHandler(IUserRepository users, TimeProvider time) => (_users, _time) = (users, time);

    public async Task<IReadOnlyList<UserDto>> HandleAsync(SearchUsersQuery query, CancellationToken ct)
    {
        if (query.Skip < 0 || query.Take is < 1 or > AccountPolicy.MaxUsersPerPage)
            throw ValidationError.For(
                "paginacion",
                $"Se pueden pedir entre 1 y {AccountPolicy.MaxUsersPerPage} usuarios desde una posición no negativa.");

        var users = await _users.SearchAsync(query.Search, query.Skip, query.Take, ct);
        var now = _time.GetUtcNow();

        return [.. users.Select(user => UserDto.From(user, now))];
    }
}
