using NotificationLog.ApiService.Contracts.Triggers;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Application.Triggers.Commands.CreateTrigger;
using NotificationLog.NotificationService.Application.Triggers.Dtos;
using NotificationLog.NotificationService.Application.Triggers.Queries.GetTriggerById;
using NotificationLog.NotificationService.Application.Triggers.Queries.ListTriggers;

namespace NotificationLog.ApiService.Endpoints;

public static class TriggerEndpoints
{
    public static IEndpointRouteBuilder MapTriggers(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/triggers").WithTags("Triggers");

        group.MapPost("/", CreateAsync)
            .WithName("CreateTrigger")
            .WithSummary("Crea un trigger de notificación")
            .Produces<CreatedTriggerResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetTriggerById")
            .WithSummary("Obtiene un trigger por su identificador")
            .Produces<TriggerDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", ListAsync)
            .WithName("ListTriggers")
            .WithSummary("Lista triggers con filtros y paginación")
            .Produces<PagedResult<TriggerSummaryDto>>();

        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateTriggerRequest body,
        CreateTriggerHandler handler,
        CancellationToken ct)
    {
        var id = await handler.HandleAsync(
            new CreateTriggerCommand(body.EventKey, body.Description), ct);

        return Results.CreatedAtRoute(
            "GetTriggerById",
            new { id },
            new CreatedTriggerResponse(id));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        GetTriggerByIdHandler handler,
        CancellationToken ct)
    {
        var trigger = await handler.HandleAsync(new GetTriggerByIdQuery(id), ct);
        return Results.Ok(trigger);
    }

    private static async Task<IResult> ListAsync(
        [AsParameters] ListTriggersQuery query,
        ListTriggersHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(query, ct);
        return Results.Ok(result);
    }
}
