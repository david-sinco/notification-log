namespace NotificationLog.IdentityService.Api.Connect;

public static class OidcScopes
{
    public const string Identity = "identity";
    public const string Rentals = "rentals";
    public const string Notifications = "notifications";

    public const string IdentityAudience = "identity-api";

    public static readonly IReadOnlyDictionary<string, string> Resources = new Dictionary<string, string>
    {
        [Identity] = IdentityAudience,
        [Rentals] = "rentals-api",
        [Notifications] = "notification-api"
    };
}
