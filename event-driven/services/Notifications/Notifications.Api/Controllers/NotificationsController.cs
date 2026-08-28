using Microsoft.AspNetCore.Mvc;
using Notifications.Api.Contracts;
using Notifications.Application;
using Notifications.Application.Ports;
using Notifications.Domain;
using Notifications.Infrastructure.EventLog;

namespace Notifications.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController(
    NotificationCommandService commands, INotificationRepository notifications, ContactMaterializer contacts)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<NotificationResponse>> RequestNotification(
        RequestNotificationRequest request, CancellationToken ct)
    {
        var notification = await commands.RequestNotificationAsync(
            request.UserId, request.Channel, request.IdempotencyKey, request.Body, ct);

        var response = NotificationResponse.From(notification);
        return CreatedAtAction(nameof(GetById), new { id = notification.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NotificationResponse>> GetById(Guid id, CancellationToken ct)
    {
        var notification = await notifications.FindAsync(id, ct);
        return notification is null ? NotFound() : Ok(NotificationResponse.From(notification));
    }

    /// <summary>Count of replicated users — compare against the Users service to see at a glance
    /// whether the log actually delivered everything.</summary>
    [HttpGet("replica/status")]
    public async Task<ActionResult<ReplicaStatusResponse>> ReplicaStatus(CancellationToken ct)
    {
        await contacts.WaitUntilReadyAsync(ct);
        return Ok(new ReplicaStatusResponse(contacts.Count()));
    }

    [HttpGet("replica/{userId:guid}")]
    public async Task<ActionResult<UserContact>> ReplicaByUser(Guid userId, CancellationToken ct)
    {
        await contacts.WaitUntilReadyAsync(ct);
        var contact = contacts.TryGet(userId);
        return contact is null ? NotFound() : Ok(contact);
    }
}
