namespace NotificationLog.RentalService.Application.Owners.Queries.Dtos;

public sealed record OwnerDto(
    Guid Id,
    Guid CreatedBy,
    Guid? RelatedUserId,
    string Name,
    string Email,
    string Phone,
    DateTimeOffset RegisteredAt,
    int ListingCount);
