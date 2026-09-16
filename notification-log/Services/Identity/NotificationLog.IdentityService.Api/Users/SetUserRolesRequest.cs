using NotificationLog.IdentityService.Api.Accounts;

namespace NotificationLog.IdentityService.Api.Users;

public sealed record SetUserRolesRequest(IReadOnlyList<UserRole> Roles);
