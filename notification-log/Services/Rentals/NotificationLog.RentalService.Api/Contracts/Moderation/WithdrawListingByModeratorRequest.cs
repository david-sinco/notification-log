namespace NotificationLog.RentalService.Api.Contracts.Moderation;

public sealed record WithdrawListingByModeratorRequest(Guid ModeratorId, string Reason);
