using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Domain.Offers.Enums;

namespace NotificationLog.RentalService.Domain.Offers.Events;

public sealed record OfferCountered(
    OfferParty By,
    long Amount,
    DateTimeOffset RespondBy
) : DomainEvent;
