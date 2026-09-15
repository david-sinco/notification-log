using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Application.Listings.Commands.DraftListing;

public sealed record DraftListingCommand(Guid ActorId, Guid PublisherId, Guid? AdvisorId, Operation Operation);
