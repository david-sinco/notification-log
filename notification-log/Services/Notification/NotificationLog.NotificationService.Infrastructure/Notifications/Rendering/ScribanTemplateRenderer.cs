using NotificationLog.NotificationService.Application.Notifications.Services.Dispatch;
using NotificationLog.NotificationService.Application.Notifications.Services.Rendering;
using NotificationLog.NotificationService.Domain.Notifications;
using NotificationLog.NotificationService.Domain.Recipients;
using Scriban;
using Scriban.Runtime;

namespace NotificationLog.NotificationService.Infrastructure.Notifications.Rendering;

// Scriban resuelve "{{ recipient.name }}" navegando un árbol de objetos (member access), no una
// clave plana con un punto adentro — por eso BuildData arma un ScriptObject anidado
// (recipient.* aparte, los campos del propio evento sueltos) en vez de un diccionario plano.
public sealed class ScribanTemplateRenderer : ITemplateRenderer
{
    public object BuildData(BusinessEvent businessEvent, Recipient recipient)
    {
        var recipientData = new ScriptObject
        {
            ["id"] = recipient.Id,
            ["name"] = recipient.Name,
            ["email"] = recipient.Email,
            ["phone"] = recipient.Phone,
            ["locale"] = recipient.Locale,
            ["timeZone"] = recipient.TimeZone
        };

        foreach (var (key, value) in recipient.Attributes)
            recipientData[key] = value;

        var root = new ScriptObject { ["recipient"] = recipientData };

        foreach (var (key, value) in businessEvent.Data)
            root[key] = value;

        return root;
    }

    public object BuildVerificationCodeData(string code)
        => new ScriptObject { ["code"] = code };

    public RenderedMessage Render(string? subjectTemplate, string bodyTemplate, object data)
    {
        var globals = (ScriptObject)data;

        var body = RenderTemplate(bodyTemplate, globals);
        var subject = subjectTemplate is null ? null : RenderTemplate(subjectTemplate, globals);

        return RenderedMessage.Create(subject, body);
    }

    private static string RenderTemplate(string templateSource, ScriptObject globals)
    {
        var template = Template.Parse(templateSource);

        if (template.HasErrors)
            throw new InvalidOperationException(
                $"Plantilla inválida: {string.Join("; ", template.Messages)}");

        // StrictVariables: un placeholder que no matchea ningún dato disponible revienta en vez de
        // renderizar vacío en silencio — mismo criterio que el resto del flujo de dispatch (todo
        // fallo queda registrado como notificación fallida, nunca se pierde sin rastro).
        var context = new TemplateContext { StrictVariables = true };
        context.PushGlobal(globals);

        return template.Render(context);
    }
}
