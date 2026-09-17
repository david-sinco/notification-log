using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NotificationLog.NotificationService.Application.Notifications.Services.Rendering;
using NotificationLog.NotificationService.Application.Notifications.Services.Sending;
using NotificationLog.NotificationService.Domain.Notifications;
using NotificationLog.NotificationService.Domain.Shared;
using NotificationLog.NotificationService.Domain.Templates;

namespace NotificationLog.NotificationService.Application.Notifications.Commands.SendVerificationCode;

public sealed class SendVerificationCodeHandler
{
    private readonly INotificationTemplateRepository _templates;
    private readonly INotificationRepository _notifications;
    private readonly ITemplateRenderer _renderer;
    private readonly IEmailNotificationSender _emailSender;
    private readonly ISmsNotificationSender _smsSender;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<SendVerificationCodeCommand> _validator;
    private readonly ILogger<SendVerificationCodeHandler> _log;

    public SendVerificationCodeHandler(
        INotificationTemplateRepository templates,
        INotificationRepository notifications,
        ITemplateRenderer renderer,
        IEmailNotificationSender emailSender,
        ISmsNotificationSender smsSender,
        IUnitOfWork uow,
        IValidator<SendVerificationCodeCommand> validator,
        ILogger<SendVerificationCodeHandler> log)
        => (_templates, _notifications, _renderer, _emailSender, _smsSender, _uow, _validator, _log)
            = (templates, notifications, renderer, emailSender, smsSender, uow, validator, log);

    public async Task<Guid> HandleAsync(SendVerificationCodeCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var templateId = SystemTemplates.VerificationCodeFor(cmd.Channel)!.Value;

        var template = await _templates.GetByIdAsync(templateId, ct)
            ?? throw new NotFoundException(nameof(NotificationTemplate), templateId);

        var version = template.CurrentVersion;

        string? providerMessageId = null;
        string? error = null;

        try
        {
            var data = _renderer.BuildVerificationCodeData(cmd.Code);
            var message = _renderer.Render(version.Subject, version.Body, data);

            var result = await SendAsync(cmd.Channel, cmd.Destination, message, ct);

            if (result.Succeeded)
                providerMessageId = result.ProviderMessageId;
            else
                error = result.Error ?? "Error desconocido";
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Fallo al enviar el código de verificación por {Channel}", cmd.Channel);
            error = ex.Message;
        }

        var notification = Notification.RecordVerificationCode(
            cmd.EventId,
            template.Id,
            version.Id,
            cmd.Channel,
            cmd.Destination,
            providerMessageId,
            error,
            DateTime.UtcNow);

        await _notifications.AddAsync(notification, ct);
        await _uow.SaveChangesAsync(ct);

        return notification.Id;
    }

    private Task<NotificationSendResult> SendAsync(
        NotificationChannel channel, string destination, RenderedMessage message, CancellationToken ct)
        => channel is NotificationChannel.Email
            ? _emailSender.SendAsync(destination, message, ct)
            : _smsSender.SendAsync(destination, message, ct);
}
