namespace NotificationLog.RentalService.Application.Abstractions;

public sealed record AdvisorInfo(Guid AdvisorId, bool IsActive, IReadOnlyCollection<string> ServiceCities, int Capacity);
