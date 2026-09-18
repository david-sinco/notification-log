using Domain.Shared.Authorization;

namespace NotificationLog.IdentityService.Api.Users;

public sealed record SetUserRolesRequest(IReadOnlyList<UserRole> Roles);
