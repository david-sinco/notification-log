using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Offers.Events;

public sealed record OfferRejected(string Reason) : DomainEvent;
