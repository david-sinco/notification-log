using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NotificationLog.IdentityService.Application.Users.Commands.LockUser;
using NotificationLog.IdentityService.Application.Users.Commands.RegisterAccount;
using NotificationLog.IdentityService.Application.Users.Commands.ResendVerificationCode;
using NotificationLog.IdentityService.Application.Users.Commands.SetUserRoles;
using NotificationLog.IdentityService.Application.Users.Commands.SignIn;
using NotificationLog.IdentityService.Application.Users.Commands.UnlockUser;
using NotificationLog.IdentityService.Application.Users.Commands.VerifyAccount;
using NotificationLog.IdentityService.Application.Users.Common;
using NotificationLog.IdentityService.Application.Users.Queries.GetUserById;
using NotificationLog.IdentityService.Application.Users.Queries.SearchUsers;
using NotificationLog.IdentityService.Application.Users.Queries.ValidateSession;

namespace NotificationLog.IdentityService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<VerificationCodeSender>();

        services.AddScoped<RegisterAccountHandler>();
        services.AddScoped<ResendVerificationCodeHandler>();
        services.AddScoped<VerifyAccountHandler>();
        services.AddScoped<SignInHandler>();
        services.AddScoped<SetUserRolesHandler>();
        services.AddScoped<LockUserHandler>();
        services.AddScoped<UnlockUserHandler>();

        services.AddScoped<GetUserByIdHandler>();
        services.AddScoped<SearchUsersHandler>();
        services.AddScoped<ValidateSessionHandler>();

        return services;
    }
}
