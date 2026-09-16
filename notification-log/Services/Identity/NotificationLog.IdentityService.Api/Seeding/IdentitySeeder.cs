using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NotificationLog.IdentityService.Api.Accounts;
using NotificationLog.IdentityService.Api.Connect;
using NotificationLog.IdentityService.Api.Data;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace NotificationLog.IdentityService.Api.Seeding;

public sealed class IdentitySeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IdentitySeedOptions _options;

    public IdentitySeeder(IServiceScopeFactory scopes, IOptions<IdentitySeedOptions> options)
        => (_scopes, _options) = (scopes, options.Value);

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

    private static async Task SeedScopesAsync(IOpenIddictScopeManager scopes, CancellationToken ct)
    {
        foreach (var (name, resource) in OidcScopes.Resources)
        {
            if (await scopes.FindByNameAsync(name, ct) is not null)
                continue;

            await scopes.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = name,
                Resources = { resource }
            }, ct);
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
