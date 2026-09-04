using NotificationLog.NotificationService.Application.Notifications.Services.Dispatch;
using NotificationLog.NotificationService.Domain.Notifications;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Notifications.Services.Rendering;

public interface ITemplateRenderer
{
    // Cada motor de plantillas necesita sus datos en una forma distinta (diccionario plano,
    // árbol de objetos anidado, un ScriptObject de Scriban, etc.) — NotificationDispatchService no
    // interpreta ni transforma nada, solo junta evento+destinatario y reenvía lo que
    // BuildData le devuelva a Render, sin saber qué hay adentro.
    object BuildData(BusinessEvent businessEvent, Recipient recipient);

    RenderedMessage Render(string? subjectTemplate, string bodyTemplate, object data);
}
