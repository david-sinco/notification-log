namespace NotificationLog.Web.Api.Rentals.Owners;

public sealed record OwnerDto(
    Guid Id,
    Guid CreatedBy,
    Guid? RelatedUserId,
    string Name,
    string Email,
    string Phone,
    DateTimeOffset RegisteredAt,
    int ListingCount);

public sealed record RegisterOwnerRequest(string Name, string Email, string Phone);
