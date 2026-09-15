using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Offers.Events;

public sealed record OfferFellThrough(string Reason) : DomainEvent;
