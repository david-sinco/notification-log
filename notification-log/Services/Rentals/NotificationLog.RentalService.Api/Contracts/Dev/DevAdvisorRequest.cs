namespace NotificationLog.RentalService.Api.Contracts.Dev;

public sealed record DevAdvisorRequest(bool IsActive, IReadOnlyList<string> ServiceCities, int Capacity);
