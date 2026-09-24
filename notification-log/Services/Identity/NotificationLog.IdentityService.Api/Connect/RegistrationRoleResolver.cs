using Domain.Shared.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using NotificationLog.IdentityService.Infrastructure.Security;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace NotificationLog.IdentityService.Api.Connect;

public sealed class RegistrationRoleResolver
{
    private readonly IOpenIddictApplicationManager _applications;

    public RegistrationRoleResolver(IOpenIddictApplicationManager applications) => _applications = applications;

    public async Task<UserRole> ResolveAsync(string? returnUrl, CancellationToken ct)
    {
        var clientId = ClientIdFrom(returnUrl);
        var application = clientId is null ? null : await _applications.FindByClientIdAsync(clientId, ct);

        if (application is null)
            return UserRole.Visitor;

        var settings = await _applications.GetSettingsAsync(application, ct);

        return settings.TryGetValue(OidcClientSettings.RegistrationRole, out var value)
            && Enum.TryParse<UserRole>(value, out var role)
                ? role
                : UserRole.Visitor;
    }

    private static string? ClientIdFrom(string? returnUrl)
    {
        var start = returnUrl?.IndexOf('?') ?? -1;

        if (start < 0)
            return null;

        return QueryHelpers.ParseQuery(returnUrl![start..]).TryGetValue(Parameters.ClientId, out var clientId)
            ? clientId.ToString()
            : null;
    }
}
