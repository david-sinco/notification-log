using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Domain.Recipients;
using NotificationLog.NotificationService.Domain.Templates;
using NotificationLog.NotificationService.Domain.Triggers;
using NotificationLog.NotificationService.Infrastructure.Persistence.Context;
using NotificationLog.NotificationService.Infrastructure.Persistence.Repositories;

namespace NotificationLog.NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Falta la cadena de conexión 'Default'.");

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<NotificationDbContext>());

        services.AddScoped<INotificationTriggerRepository, NotificationTriggerRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<IRecipientRepository, RecipientRepository>();

        return services;
    }
}