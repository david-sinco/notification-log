using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Application.Recipients.Dtos;
using NotificationLog.NotificationService.Application.Recipients.Queries.GetRecipientById;
using NotificationLog.NotificationService.Application.Recipients.Queries.ListRecipients;

namespace NotificationLog.ApiService.Endpoints;

public static class RecipientEndpoints
{
    public static IEndpointRouteBuilder MapRecipients(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/recipients").WithTags("Recipients");

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetRecipientById")
            .WithSummary("Obtiene un destinatario por su identificador")
            .Produces<RecipientDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", ListAsync)
            .WithName("ListRecipients")
            .WithSummary("Lista destinatarios con filtros y paginación")
            .Produces<PagedResult<RecipientSummaryDto>>();

        return app;
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        GetRecipientByIdHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(new GetRecipientByIdQuery(id), ct));

    private static async Task<IResult> ListAsync(
        [AsParameters] ListRecipientsQuery query,
        ListRecipientsHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(query, ct));
}