using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Api.Contracts.Listings;

public sealed record DraftListingRequest(Guid ActorId, Guid PublisherId, Guid? AdvisorId, Operation Operation);
