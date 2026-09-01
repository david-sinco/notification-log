using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NotificationLog.NotificationService.Application.Recipients.Commands.CreateRecipient;
using NotificationLog.NotificationService.Application.Recipients.Commands.SetRecipientStatus;
using NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientAttributes;
using NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientEmail;
using NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientPhone;
using NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientProfile;
using NotificationLog.NotificationService.Application.Recipients.Queries.GetRecipientById;
using NotificationLog.NotificationService.Application.Recipients.Queries.ListRecipients;
using NotificationLog.NotificationService.Application.Templates.Commands.CreateTemplate;
using NotificationLog.NotificationService.Application.Templates.Commands.SetTemplateStatus;
using NotificationLog.NotificationService.Application.Templates.Commands.UpdateTemplateContent;
using NotificationLog.NotificationService.Application.Templates.Queries.GetTemplateById;
using NotificationLog.NotificationService.Application.Templates.Queries.ListTemplates;
using NotificationLog.NotificationService.Application.Triggers.Commands.AddConfiguration;
using NotificationLog.NotificationService.Application.Triggers.Commands.ChangeConfigurationTemplate;
using NotificationLog.NotificationService.Application.Triggers.Commands.CreateTrigger;
using NotificationLog.NotificationService.Application.Triggers.Commands.DisableConfiguration;
using NotificationLog.NotificationService.Application.Triggers.Commands.EnableConfiguration;
using NotificationLog.NotificationService.Application.Triggers.Commands.SetTriggerStatus;
using NotificationLog.NotificationService.Application.Triggers.Commands.UpdateTriggerDescription;
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
        services.AddScoped<AddConfigurationHandler>();
        services.AddScoped<DisableConfigurationHandler>();
        services.AddScoped<EnableConfigurationHandler>();
        services.AddScoped<ChangeConfigurationTemplateHandler>();
        services.AddScoped<SetTriggerStatusHandler>();
        services.AddScoped<UpdateTriggerDescriptionHandler>();


        // Templates
        services.AddScoped<CreateTemplateHandler>();
        services.AddScoped<UpdateTemplateContentHandler>();
        services.AddScoped<SetTemplateStatusHandler>();
        services.AddScoped<GetTemplateByIdHandler>();
        services.AddScoped<ListTemplatesHandler>();

        // Recipients
        services.AddScoped<CreateRecipientHandler>();
        services.AddScoped<UpdateRecipientEmailHandler>();
        services.AddScoped<UpdateRecipientPhoneHandler>();
        services.AddScoped<UpdateRecipientProfileHandler>();
        services.AddScoped<UpdateRecipientAttributesHandler>();
        services.AddScoped<SetRecipientStatusHandler>();
        services.AddScoped<GetRecipientByIdHandler>();
        services.AddScoped<ListRecipientsHandler>();

        return services;
    }
}