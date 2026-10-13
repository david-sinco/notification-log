namespace NotificationLog.IdentityService.Infrastructure.Seeding;

public sealed class LoadTestSeedOptions
{
    public bool Enabled { get; init; }
    public string ClientId { get; init; } = "loadtest";
    public string Password { get; init; } = "LoadTest12345";
    public string EmailDomain { get; init; } = "loadtest.local";
    public int Visitors { get; init; } = 1000;
    public int Owners { get; init; } = 100;
    public int Moderators { get; init; } = 5;
    public int Administrators { get; init; } = 1;
}
