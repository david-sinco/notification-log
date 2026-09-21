using Domain.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotificationLog.IdentityService.Domain.Users;
using NotificationLog.IdentityService.Domain.Users.Enums;
using NotificationLog.IdentityService.Domain.Users.ValueObjects;

namespace NotificationLog.IdentityService.Infrastructure.Persistence;

internal sealed class IdentityUserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IdentityServiceDbContext _db;

    public IdentityUserRepository(UserManager<ApplicationUser> users, IdentityServiceDbContext db)
        => (_users, _db) = (users, db);

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await _users.FindByIdAsync(id.ToString());

        return entity is null ? null : await ToDomainAsync(entity);
    }

    public async Task<User?> FindByLoginAsync(LoginIdentifier login, CancellationToken ct)
    {
        var entity = login.Channel == LoginChannel.Email
            ? await _users.FindByEmailAsync(login.Value)
            : await _users.Users.SingleOrDefaultAsync(user => user.PhoneNumber == login.Value, ct);

        return entity is null ? null : await ToDomainAsync(entity);
    }

    public async Task<IReadOnlyList<User>> SearchAsync(string? term, int skip, int take, CancellationToken ct)
    {
        var query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(term))
        {
            var search = term.Trim();
            var normalizedEmail = _users.NormalizeEmail(search);

            query = query.Where(user =>
                (user.NormalizedEmail != null && user.NormalizedEmail.Contains(normalizedEmail))
                || (user.PhoneNumber != null && user.PhoneNumber.Contains(search)));
        }

        var page = await query
            .OrderBy(user => user.NormalizedEmail ?? user.PhoneNumber)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        var ids = page.Select(user => user.Id).ToList();

        var roles = await (
                from userRole in _db.UserRoles
                join role in _db.Roles on userRole.RoleId equals role.Id
                where ids.Contains(userRole.UserId)
                select new { userRole.UserId, role.Name })
            .ToListAsync(ct);

        var rolesByUser = roles.ToLookup(role => role.UserId, role => role.Name!);

        return [.. page.Select(user => UserMapping.ToDomain(user, rolesByUser[user.Id]))];
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        var created = await _users.CreateAsync(UserMapping.ToEntity(user));

        if (!created.Succeeded)
            throw new DomainException(string.Join(" ", created.Errors.Select(error => error.Description)));
    }

    public async Task UpdateAsync(User user, CancellationToken ct)
    {
        var entity = await _users.FindByIdAsync(user.Id.ToString())
            ?? throw new DomainException($"La cuenta '{user.Id}' ya no existe.");

        if (!string.Equals(entity.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            var updated = await _users.SetEmailAsync(entity, user.Email);

            if (!updated.Succeeded)
                throw new DomainException(string.Join(" ", updated.Errors.Select(error => error.Description)));
        }

        UserMapping.Apply(user, entity);
    }

    private async Task<User> ToDomainAsync(ApplicationUser entity) =>
        UserMapping.ToDomain(entity, await _users.GetRolesAsync(entity));
}
