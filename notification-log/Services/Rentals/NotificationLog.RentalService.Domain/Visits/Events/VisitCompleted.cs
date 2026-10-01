using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Domain.Visits.Events;

public sealed record VisitCompleted(VisitParty By) : DomainEvent;
