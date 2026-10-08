using Domain.Shared.Authorization;

namespace NotificationLog.RentalService.IntegrationTests.Support;

public sealed record TestUser(string Name, Guid Id, UserRole Role)
{
    public string Email => $"{Name.ToLowerInvariant()}@example.com";

    public string Phone => "+573109876543";
}
