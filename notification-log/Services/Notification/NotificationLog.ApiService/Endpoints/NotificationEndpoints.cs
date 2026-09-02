using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Application.Notifications.Dtos;
using NotificationLog.NotificationService.Application.Notifications.Queries.ListNotifications;

namespace NotificationLog.ApiService.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotifications(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications").WithTags("Notifications");

        group.MapGet("/", ListAsync)
            .WithName("ListNotifications")
            .WithSummary("Lista el historial de notificaciones enviadas, con filtros y paginación")
            .Produces<PagedResult<NotificationDto>>();

        return app;
    }

    private static async Task<IResult> ListAsync(
        [AsParameters] ListNotificationsQuery query,
        ListNotificationsHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(query, ct));
}
