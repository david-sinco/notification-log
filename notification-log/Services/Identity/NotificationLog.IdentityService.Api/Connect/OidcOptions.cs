namespace NotificationLog.IdentityService.Api.Connect;

public sealed class OidcOptions
{
    public const string SectionName = "Oidc";

    public string Issuer { get; init; } = string.Empty;
    public Dictionary<string, string> Audiences { get; init; } = [];
}
