using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Domain.Visits.Events;

public sealed record VisitCounterProposed(VisitParty By, IReadOnlyList<DateTimeOffset> Slots, DateTimeOffset RespondBy) : DomainEvent;
