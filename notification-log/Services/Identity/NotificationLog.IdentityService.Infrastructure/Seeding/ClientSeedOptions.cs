namespace NotificationLog.IdentityService.Infrastructure.Seeding;

public sealed class ClientSeedOptions
{
    public string ClientId { get; init; } = string.Empty;
    public string? ClientSecret { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public List<string> RedirectUris { get; init; } = [];
    public List<string> PostLogoutRedirectUris { get; init; } = [];
    public List<string> Scopes { get; init; } = [];
}
