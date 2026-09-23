using Application.Shared.Pagination;
using NotificationLog.RentalService.Api.Contracts.Visits;
using NotificationLog.RentalService.Application.Visits.Queries;
using NotificationLog.RentalService.Application.Visits.Queries.Dtos;
using NotificationLog.RentalService.Application.Visits.Queries.Filters;
using NotificationLog.RentalService.Application.Visits.Commands.CancelVisit;
using NotificationLog.RentalService.Application.Visits.Commands.ConfirmVisit;
using NotificationLog.RentalService.Application.Visits.Commands.DeclineVisit;
using NotificationLog.RentalService.Application.Visits.Commands.ReportVisitOutcome;
using NotificationLog.RentalService.Application.Visits.Commands.RequestVisit;

namespace NotificationLog.RentalService.Api.Endpoints;

public static class VisitEndpoints
{
    public static IEndpointRouteBuilder MapVisits(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/visits").WithTags("Visits");

        group.MapPost("/", RequestAsync)
            .WithName("RequestVisit")
            .WithSummary("Solicita una visita proponiendo hasta tres franjas")
            .Produces<CreatedVisitResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/", ListAsync)
            .WithName("ListVisits")
            .WithSummary("Lista visitas con filtros y paginación")
            .Produces<PagedResult<VisitSummaryDto>>();

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetVisitById")
            .WithSummary("Obtiene una visita por su identificador")
            .Produces<VisitDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/confirm", ConfirmAsync)
            .WithName("ConfirmVisit")
            .WithSummary("Confirma una de las franjas propuestas")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/decline", DeclineAsync)
            .WithName("DeclineVisit")
            .WithSummary("Rechaza la solicitud de visita")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/cancel", CancelAsync)
            .WithName("CancelVisit")
            .WithSummary("Cancela la visita")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/outcome", ReportOutcomeAsync)
            .WithName("ReportVisitOutcome")
            .WithSummary("Marca la visita como realizada o como inasistencia")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return app;
    }

    private static async Task<IResult> RequestAsync(
        RequestVisitRequest body,
        RequestVisitHandler handler,
        CancellationToken ct)
    {
        var id = await handler.HandleAsync(new RequestVisitCommand(body.VisitorId, body.ListingId, body.SlotStarts), ct);
        return Results.Created($"/api/visits/{id}", new CreatedVisitResponse(id));
    }

    private static async Task<IResult> ListAsync(
        [AsParameters] VisitFilter filter,
        [AsParameters] PageRequest paging,
        ListVisitsHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(filter, paging, ct));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        GetVisitByIdHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(id, ct));

    private static async Task<IResult> ConfirmAsync(
        Guid id,
        ConfirmVisitRequest body,
        ConfirmVisitHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new ConfirmVisitCommand(body.ActorId, id, body.SlotStart), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> DeclineAsync(
        Guid id,
        DeclineVisitRequest body,
        DeclineVisitHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new DeclineVisitCommand(body.ActorId, id, body.Reason), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> CancelAsync(
        Guid id,
        CancelVisitRequest body,
        CancelVisitHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new CancelVisitCommand(body.ActorId, id, body.Reason), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ReportOutcomeAsync(
        Guid id,
        ReportVisitOutcomeRequest body,
        ReportVisitOutcomeHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new ReportVisitOutcomeCommand(body.ActorId, id, body.Attended), ct);
        return Results.NoContent();
    }
}
