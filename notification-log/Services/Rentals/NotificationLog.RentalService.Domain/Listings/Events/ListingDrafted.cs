using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Domain.Listings.Events;

public sealed record ListingDrafted(
    Guid ListingId,
    Guid OwnerId,
    Guid CreatedBy,
    Operation Operation
) : DomainEvent;
