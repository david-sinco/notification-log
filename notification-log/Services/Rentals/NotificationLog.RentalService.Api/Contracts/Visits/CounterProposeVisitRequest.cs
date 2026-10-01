namespace NotificationLog.RentalService.Api.Contracts.Visits;

public sealed record CounterProposeVisitRequest(IReadOnlyList<DateTimeOffset> Slots);
