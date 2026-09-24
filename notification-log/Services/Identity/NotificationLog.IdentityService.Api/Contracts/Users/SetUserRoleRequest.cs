using Domain.Shared.Authorization;

namespace NotificationLog.IdentityService.Api.Contracts.Users;

public sealed record SetUserRoleRequest(UserRole Role);
