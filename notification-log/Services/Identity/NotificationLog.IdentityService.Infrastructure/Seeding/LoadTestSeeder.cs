using Domain.Shared.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NotificationLog.IdentityService.Application.Abstractions;
using NotificationLog.IdentityService.Domain.Users;
using NotificationLog.IdentityService.Infrastructure.Messaging;
using NotificationLog.IdentityService.Infrastructure.Persistence;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace NotificationLog.IdentityService.Infrastructure.Seeding;

public sealed class LoadTestSeeder : IHostedService
{
    private const int BatchSize = 100;

    private readonly IServiceScopeFactory _scopes;
    private readonly LoadTestSeedOptions _options;

    public LoadTestSeeder(IServiceScopeFactory scopes, IOptions<IdentitySeedOptions> options)
        => (_scopes, _options) = (scopes, options.Value.LoadTest);

    public async Task StartAsync(CancellationToken ct)
    {
        if (!_options.Enabled)
            return;

        await using var scope = _scopes.CreateAsyncScope();
        var services = scope.ServiceProvider;

        await SeedClientAsync(services.GetRequiredService<IOpenIddictApplicationManager>(), ct);
        await SeedUsersAsync(
            services.GetRequiredService<UserManager<ApplicationUser>>(),
            services.GetRequiredService<IUserEventPublisher>(),
            services.GetRequiredService<IdentityOutbox>(),
            ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    private async Task SeedClientAsync(IOpenIddictApplicationManager applications, CancellationToken ct)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = _options.ClientId,
            ClientType = ClientTypes.Public,
            DisplayName = "Pruebas de carga",
            Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.Password,
                Permissions.Scopes.Email,
                Permissions.Scopes.Phone,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope + OidcScope.Identity.ToScopeName(),
                Permissions.Prefixes.Scope + OidcScope.Rentals.ToScopeName(),
                Permissions.Prefixes.Scope + OidcScope.Notifications.ToScopeName()
            }
        };

        var existing = await applications.FindByClientIdAsync(_options.ClientId, ct);

        if (existing is null)
            await applications.CreateAsync(descriptor, ct);
        else
            await applications.UpdateAsync(existing, descriptor, ct);
    }

    private async Task SeedUsersAsync(
        UserManager<ApplicationUser> users,
        IUserEventPublisher events,
        IdentityOutbox outbox,
        CancellationToken ct)
    {
        var suffix = "@" + _options.EmailDomain;

        var existing = (await users.Users
                .Where(user => user.Email != null && user.Email.EndsWith(suffix))
                .Select(user => user.Email!)
                .ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var passwordHash = users.PasswordHasher.HashPassword(new ApplicationUser(), _options.Password);
        var pending = 0;

        foreach (var (email, name, role) in Accounts())
        {
            if (existing.Contains(email))
                continue;

            var entity = new ApplicationUser(Guid.NewGuid())
            {
                Name = name,
                Email = email,
                EmailConfirmed = true,
                PasswordHash = passwordHash,
                Locale = AccountPolicy.DefaultLocale,
                TimeZone = AccountPolicy.DefaultTimeZone,
                AcceptsNotifications = true
            };

            var created = await users.CreateAsync(entity);

            if (!created.Succeeded)
                throw new InvalidOperationException(
                    $"No se pudo crear el usuario de prueba '{email}': {string.Join(" ", created.Errors.Select(e => e.Description))}");

            await users.AddToRoleAsync(entity, role.ToString());
            await events.PublishUserCreatedAsync(UserMapping.ToDomain(entity, [role.ToString()]), ct);

            if (++pending == BatchSize)
            {
                await outbox.SaveChangesAndFlushAsync(ct);
                pending = 0;
            }
        }

        if (pending > 0)
            await outbox.SaveChangesAndFlushAsync(ct);
    }

    private IEnumerable<(string Email, string Name, UserRole Role)> Accounts()
    {
        foreach (var account in Range("visitor", "Visitante", _options.Visitors, UserRole.Visitor))
            yield return account;

        foreach (var account in Range("owner", "Propietario", _options.Owners, UserRole.Propietario))
            yield return account;

        foreach (var account in Range("moderator", "Moderador", _options.Moderators, UserRole.Moderador))
            yield return account;

        foreach (var account in Range("admin", "Administrador", _options.Administrators, UserRole.Administrador))
            yield return account;
    }

    private IEnumerable<(string Email, string Name, UserRole Role)> Range(string prefix, string label, int count, UserRole role)
    {
        for (var i = 1; i <= count; i++)
            yield return ($"{prefix}-{i:D4}@{_options.EmailDomain}", $"{label} de carga {i:D4}", role);
    }
}
