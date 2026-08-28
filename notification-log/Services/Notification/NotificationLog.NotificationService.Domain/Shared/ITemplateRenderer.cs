using NotificationLog.NotificationService.Domain.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationLog.NotificationService.Domain.Shared;


public interface ITemplateRenderer
{
    RenderedMessage Render(
        //NotificationTemplate template,
        //Recipient recipient,
        IReadOnlyDictionary<string, object?> eventData);
}
