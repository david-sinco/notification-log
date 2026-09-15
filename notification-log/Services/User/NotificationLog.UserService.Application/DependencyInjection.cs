using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NotificationLog.UserService.Application.Users.Commands.ConfirmEmail;
using NotificationLog.UserService.Application.Users.Commands.ConfirmPhone;
using NotificationLog.UserService.Application.Users.Commands.RegisterUser;
using NotificationLog.UserService.Application.Users.Commands.SetUserStatus;
using NotificationLog.UserService.Application.Users.Commands.UpdateProfile;

namespace NotificationLog.UserService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly,
            includeInternalTypes: true);

        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<UpdateProfileHandler>();
        services.AddScoped<ConfirmEmailHandler>();
        services.AddScoped<ConfirmPhoneHandler>();
        services.AddScoped<SetUserStatusHandler>();

        return services;
    }
}
