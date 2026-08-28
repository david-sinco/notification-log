using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NotificationLog.NotificationService.Application.Templates.Commands.CreateTemplate;
using NotificationLog.NotificationService.Application.Templates.Commands.SetTemplateStatus;
using NotificationLog.NotificationService.Application.Templates.Commands.UpdateTemplateContent;
using NotificationLog.NotificationService.Application.Templates.Queries.GetTemplateById;
using NotificationLog.NotificationService.Application.Templates.Queries.ListTemplates;
using NotificationLog.NotificationService.Application.Triggers.Commands.CreateTrigger;
using NotificationLog.NotificationService.Application.Triggers.Queries.GetTriggerById;
using NotificationLog.NotificationService.Application.Triggers.Queries.ListTriggers;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationLog.NotificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly,
            includeInternalTypes: true);

        // Triggers
        services.AddScoped<CreateTriggerHandler>();
        services.AddScoped<GetTriggerByIdHandler>();
        services.AddScoped<ListTriggersHandler>();
        //services.AddScoped<AddConfigurationHandler>();
        //services.AddScoped<DisableConfigurationHandler>();
        //services.AddScoped<GetTriggerByIdHandler>();


        // Templates
        services.AddScoped<CreateTemplateHandler>();
        services.AddScoped<UpdateTemplateContentHandler>();
        services.AddScoped<SetTemplateStatusHandler>();
        services.AddScoped<GetTemplateByIdHandler>();
        services.AddScoped<ListTemplatesHandler>();

        return services;
    }
}