using Application.Shared.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NotificationLog.IdentityService.Application.Abstractions;
using NotificationLog.IdentityService.Domain.Users;
using NotificationLog.IdentityService.Infrastructure.Messaging;
using NotificationLog.IdentityService.Infrastructure.Persistence;
using NotificationLog.IdentityService.Infrastructure.Security;
using NotificationLog.IdentityService.Infrastructure.Seeding;
using Wolverine.EntityFrameworkCore;

namespace NotificationLog.IdentityService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("identity-db")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'identity-db'.");

        services.Configure<OidcOptions>(configuration.GetSection(OidcOptions.SectionName));
        services.Configure<IdentitySeedOptions>(configuration.GetSection(IdentitySeedOptions.SectionName));

        services.AddDbContextWithWolverineIntegration<IdentityServiceDbContext>(
            options => options.UseNpgsql(connectionString),
            RabbitMqMessagingExtensions.WolverineSchema);

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = false;

                options.Password.RequiredLength = AccountPolicy.PasswordMinLength;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = AccountPolicy.MaxFailedAttempts;
                options.Lockout.DefaultLockoutTimeSpan = AccountPolicy.FailedAttemptsLockout;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<IdentityServiceDbContext>()
            .AddTokenProvider<EmailTokenProvider<ApplicationUser>>(TokenOptions.DefaultEmailProvider)
            .AddTokenProvider<PhoneNumberTokenProvider<ApplicationUser>>(TokenOptions.DefaultPhoneProvider);

        services.AddOpenIddict()
            .AddCore(options => options
                .UseEntityFrameworkCore()
                .UseDbContext<IdentityServiceDbContext>());

        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<IUserRepository, IdentityUserRepository>();
        services.AddScoped<IUserSecurity, IdentityUserSecurity>();
        services.AddScoped<ISessionRevoker, OpenIddictSessionRevoker>();
        services.AddScoped<IUserEventPublisher, WolverineUserEventPublisher>();
        services.AddScoped<IUnitOfWork, OutboxUnitOfWork>();

        services.AddRabbitMqMessaging(configuration);

        services.AddScoped<IdentityOutbox>();
        services.AddHostedService<IdentitySeeder>();

        return services;
    }

    public static async Task MigrateIdentityDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();

        await db.Database.MigrateAsync(ct);
    }
}
