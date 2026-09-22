namespace Domain.Shared.Authorization;

public enum OidcScope
{
    Identity,
    Rentals,
    Notifications
}

public static class OidcScopeExtensions
{
    public static string ToScopeName(this OidcScope scope) => scope switch
    {
        OidcScope.Identity => "identity",
        OidcScope.Rentals => "rentals",
        OidcScope.Notifications => "notifications",
        _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Scope OIDC desconocido.")
    };
}