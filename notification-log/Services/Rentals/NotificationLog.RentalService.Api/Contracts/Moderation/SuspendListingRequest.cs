namespace NotificationLog.RentalService.Api.Contracts.Moderation;

public sealed record SuspendListingRequest(Guid ModeratorId, string Reason);
