namespace NotificationLog.RentalService.Application.Listings.Commands.AssignAdvisor;

public sealed record AssignAdvisorCommand(Guid ActorId, Guid ListingId, Guid? AdvisorId);
