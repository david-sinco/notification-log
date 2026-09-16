namespace NotificationLog.IdentityService.Api.Seeding;

public sealed class IdentitySeedOptions
{
    public const string SectionName = "Seed";

    public AdminSeedOptions Admin { get; init; } = new();
    public List<ClientSeedOptions> Clients { get; init; } = [];
}
