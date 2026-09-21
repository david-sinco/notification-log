using Domain.Shared.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NotificationLog.IdentityService.Domain.Users.Enums;
using NotificationLog.IdentityService.Domain.Users.ValueObjects;
using NotificationLog.IdentityService.Infrastructure.Persistence;
using NotificationLog.IdentityService.Infrastructure.Security;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace NotificationLog.IdentityService.Infrastructure.Seeding;

public sealed class IdentitySeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IdentitySeedOptions _options;
    private readonly OidcOptions _oidc;

    public IdentitySeeder(IServiceScopeFactory scopes, IOptions<IdentitySeedOptions> options, IOptions<OidcOptions> oidc)
        => (_scopes, _options, _oidc) = (scopes, options.Value, oidc.Value);

    public async Task StartAsync(CancellationToken ct)
    {
        await using var scope = _scopes.CreateAsyncScope();
        var services = scope.ServiceProvider;

        await SeedRolesAsync(services.GetRequiredService<RoleManager<IdentityRole<Guid>>>());
        await SeedAdminAsync(services.GetRequiredService<UserManager<ApplicationUser>>());
        await SeedScopesAsync(services.GetRequiredService<IOpenIddictScopeManager>(), ct);
        await SeedClientsAsync(services.GetRequiredService<IOpenIddictApplicationManager>(), ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roles)
    {
        foreach (var role in Enum.GetNames<UserRole>())
        {
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole<Guid>(role));
        }
    }

    private async Task SeedAdminAsync(UserManager<ApplicationUser> users)
    {
        var login = LoginIdentifier.TryParse(_options.Admin.Email);

        if (login is not { Channel: LoginChannel.Email } || string.IsNullOrWhiteSpace(_options.Admin.Password))
            return;

        if (await users.FindByEmailAsync(login.Value) is not null)
            return;

        var admin = new ApplicationUser(Guid.NewGuid())
        {
            Email = login.Value,
            EmailConfirmed = true
        };

        var created = await users.CreateAsync(admin, _options.Admin.Password);

        if (!created.Succeeded)
            throw new InvalidOperationException(
                $"No se pudo crear el administrador inicial: {string.Join(" ", created.Errors.Select(e => e.Description))}");

        await users.AddToRoleAsync(admin, nameof(UserRole.Administrador));
    }

    private async Task SeedScopesAsync(IOpenIddictScopeManager scopes, CancellationToken ct)
    {
        foreach (var (name, audience) in _oidc.Audiences)
        {
            var descriptor = new OpenIddictScopeDescriptor
            {
                Name = name,
                Resources = { audience }
            };

            var existing = await scopes.FindByNameAsync(name, ct);

            if (existing is null)
                await scopes.CreateAsync(descriptor, ct);
            else
                await scopes.UpdateAsync(existing, descriptor, ct);
        }
    }

    private async Task SeedClientsAsync(IOpenIddictApplicationManager applications, CancellationToken ct)
    {
        foreach (var client in _options.Clients)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = client.ClientId,
                ClientSecret = client.ClientSecret,
                ClientType = string.IsNullOrEmpty(client.ClientSecret) ? ClientTypes.Public : ClientTypes.Confidential,
                ConsentType = ConsentTypes.Implicit,
                DisplayName = client.DisplayName,
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.EndSession,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Phone,
                    Permissions.Scopes.Roles
                },
                Requirements = { Requirements.Features.ProofKeyForCodeExchange }
            };

            foreach (var uri in client.RedirectUris)
                descriptor.RedirectUris.Add(new Uri(uri));

            foreach (var uri in client.PostLogoutRedirectUris)
                descriptor.PostLogoutRedirectUris.Add(new Uri(uri));

            foreach (var scope in client.Scopes)
                descriptor.Permissions.Add(Permissions.Prefixes.Scope + scope);

            var existing = await applications.FindByClientIdAsync(client.ClientId, ct);

            if (existing is null)
                await applications.CreateAsync(descriptor, ct);
            else
                await applications.UpdateAsync(existing, descriptor, ct);
        }
    }
}
