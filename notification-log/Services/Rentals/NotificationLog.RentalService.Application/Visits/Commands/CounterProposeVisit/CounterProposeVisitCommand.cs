namespace NotificationLog.RentalService.Application.Visits.Commands.CounterProposeVisit;

public sealed record CounterProposeVisitCommand(Guid VisitId, IReadOnlyList<DateTimeOffset> Slots);
