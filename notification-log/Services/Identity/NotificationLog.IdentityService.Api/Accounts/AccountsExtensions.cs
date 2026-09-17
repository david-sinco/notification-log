using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotificationLog.IdentityService.Api.Data;
using NotificationLog.IdentityService.Api.Messaging;
using Wolverine.EntityFrameworkCore;

namespace NotificationLog.IdentityService.Api.Accounts;

public static class AccountsExtensions
{
    public static IServiceCollection AddAccounts(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("identity-db")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'identity-db'.");

        services.AddDbContextWithWolverineIntegration<IdentityServiceDbContext>(
            options => options.UseNpgsql(connectionString),
            MessagingExtensions.WolverineSchema);

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

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AccountService>();

        return services;
    }
}
