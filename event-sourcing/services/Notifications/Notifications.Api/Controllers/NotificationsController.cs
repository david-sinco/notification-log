using Marten;
using Microsoft.AspNetCore.Mvc;
using Notifications.Api.Contracts;
using Notifications.Application;
using Notifications.Domain;

namespace Notifications.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController(
    NotificationCommandService commands, IQuerySession session) : ControllerBase
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
        var notification = await session.LoadAsync<Notification>(id, ct);
        return notification is null ? NotFound() : Ok(NotificationResponse.From(notification));
    }

    /// <summary>Count of replicated users — compare against the Users service to see at a glance
    /// whether the transport actually delivered everything (SPEC.md §8).</summary>
    [HttpGet("replica/status")]
    public async Task<ActionResult<ReplicaStatusResponse>> ReplicaStatus(CancellationToken ct)
    {
        var count = await session.Query<UserContact>().CountAsync(ct);
        return Ok(new ReplicaStatusResponse(count));
    }

    [HttpGet("replica/{userId:guid}")]
    public async Task<ActionResult<UserContact>> ReplicaByUser(Guid userId, CancellationToken ct)
    {
        var contact = await session.LoadAsync<UserContact>(userId, ct);
        return contact is null ? NotFound() : Ok(contact);
    }
}
