namespace NotificationLog.IdentityService.Infrastructure.Seeding;

public sealed class IdentitySeedOptions
{
    public const string SectionName = "Seed";

    public AdminSeedOptions Admin { get; init; } = new();
    public List<ClientSeedOptions> Clients { get; init; } = [];
    public LoadTestSeedOptions LoadTest { get; init; } = new();
}
