using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Domain.Listings.Events;

public sealed record ListingReported(Guid ReporterId, ReportReason Reason) : DomainEvent;
