using Domain.Shared.Authorization;

namespace NotificationLog.RentalService.Application.Visitors.Commands.UpsertVisitor;

public sealed record UpsertVisitorCommand(
    Guid VisitorId,
    UserRole? Role,
    string Name,
    string Email,
    string Phone);
