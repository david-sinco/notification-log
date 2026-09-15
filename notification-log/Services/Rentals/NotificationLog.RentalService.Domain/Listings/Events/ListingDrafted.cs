using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Domain.Listings.Events;

public sealed record ListingDrafted(
    Guid ListingId,
    Guid PublisherId,
    Guid CreatedBy,
    Guid? AdvisorId,
    Operation Operation
) : DomainEvent;
