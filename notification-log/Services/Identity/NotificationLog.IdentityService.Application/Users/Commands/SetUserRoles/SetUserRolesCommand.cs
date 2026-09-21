using Domain.Shared.Authorization;

namespace NotificationLog.IdentityService.Application.Users.Commands.SetUserRoles;

public sealed record SetUserRolesCommand(Guid Id, IReadOnlyList<UserRole> Roles);
