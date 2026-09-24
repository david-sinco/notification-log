using Domain.Shared.Authorization;

namespace NotificationLog.IdentityService.Application.Users.Commands.SetUserRole;

public sealed record SetUserRoleCommand(Guid Id, UserRole Role);
