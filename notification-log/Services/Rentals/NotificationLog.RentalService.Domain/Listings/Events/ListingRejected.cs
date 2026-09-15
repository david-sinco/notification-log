using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Domain.Listings.Events;

public sealed record ListingRejected(Guid ModeratorId, IReadOnlyList<RejectionReason> Reasons) : DomainEvent;
