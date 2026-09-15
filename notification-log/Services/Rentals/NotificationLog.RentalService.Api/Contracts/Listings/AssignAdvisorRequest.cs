namespace NotificationLog.RentalService.Api.Contracts.Listings;

public sealed record AssignAdvisorRequest(Guid ActorId, Guid? AdvisorId);
