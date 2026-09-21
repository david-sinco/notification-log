using Domain.Shared.Authorization;

namespace NotificationLog.RentalService.Application.Abstractions;

public sealed record AccountRequest(
    Guid UserId,
    string Name,
    string Email,
    string Phone,
    IReadOnlyList<UserRole> Roles);
