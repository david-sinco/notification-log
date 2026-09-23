namespace Domain.Shared.Authorization;

public enum OidcScope
{
    Identity,
    Rentals,
    Notifications
}

public static class OidcScopeNames
{
    public const string Identity = "identity";
    public const string Rentals = "rentals";
    public const string Notifications = "notifications";
}

public static class OidcScopeExtensions
{
    public static string ToScopeName(this OidcScope scope) => scope switch
    {
        OidcScope.Identity => OidcScopeNames.Identity,
        OidcScope.Rentals => OidcScopeNames.Rentals,
        OidcScope.Notifications => OidcScopeNames.Rentals,
        _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Scope OIDC desconocido.")
    };
}