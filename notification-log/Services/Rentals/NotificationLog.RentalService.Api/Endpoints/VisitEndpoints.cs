using System.Security.Claims;
using API.Shared.Extensions;
using Application.Shared.Pagination;
using NotificationLog.RentalService.Api.Contracts.Visits;
using NotificationLog.RentalService.Application.Visits.Commands.CancelVisit;
using NotificationLog.RentalService.Application.Visits.Commands.CounterProposeVisit;
using NotificationLog.RentalService.Application.Visits.Commands.MarkVisitCompleted;
using NotificationLog.RentalService.Application.Visits.Commands.MarkVisitNoShow;
using NotificationLog.RentalService.Application.Visits.Commands.RequestVisit;
using NotificationLog.RentalService.Application.Visits.Commands.ScheduleVisit;
using NotificationLog.RentalService.Application.Visits.Queries;
using NotificationLog.RentalService.Application.Visits.Queries.Dtos;
using NotificationLog.RentalService.Application.Visits.Queries.Filters;

namespace NotificationLog.RentalService.Api.Endpoints;

public static class VisitEndpoints
{
    public static IEndpointRouteBuilder MapVisits(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/visits")
            .WithTags("Visits")
            .RequireAuthorization(AuthorizationExtensions.RentalsScopePolicy);

        group.MapPost("/", RequestAsync)
            .WithName("RequestVisit")
            .WithSummary("Pide una visita a una publicación proponiendo de 1 a 3 franjas")
            .Produces<CreatedVisitResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/", ListAsync)
            .WithName("ListVisits")
            .WithSummary("Lista las visitas en las que participas, o todas si eres administrador o moderador")
            .Produces<PagedResult<VisitDto>>();

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetVisitById")
            .WithSummary("Obtiene una visita en la que participas, o cualquiera si eres administrador o moderador")
            .Produces<VisitDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/counter-proposal", CounterProposeAsync)
            .WithName("CounterProposeVisit")
            .WithSummary("Propone otras franjas y pasa el turno a la otra parte")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/schedule", ScheduleAsync)
            .WithName("ScheduleVisit")
            .WithSummary("Acepta una de las franjas propuestas y agenda la visita")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/cancel", CancelAsync)
            .WithName("CancelVisit")
            .WithSummary("Cancela la visita indicando el motivo")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/complete", MarkCompletedAsync)
            .WithName("MarkVisitCompleted")
            .WithSummary("El anfitrión marca la visita como realizada")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/no-show", MarkNoShowAsync)
            .WithName("MarkVisitNoShow")
            .WithSummary("El anfitrión marca que el visitante no asistió")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return app;
    }

    private static async Task<IResult> RequestAsync(
        RequestVisitRequest body,
        ClaimsPrincipal user,
        RequestVisitHandler handler,
        CancellationToken ct)
    {
        var id = await handler.HandleAsync(new RequestVisitCommand(body.ListingId, body.Slots), user, ct);

        return Results.Created($"/api/visits/{id}", new CreatedVisitResponse(id));
    }

    private static async Task<IResult> ListAsync(
        [AsParameters] VisitFilter filter,
        [AsParameters] PageRequest paging,
        ClaimsPrincipal user,
        ListVisitsHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(filter, paging, user, ct));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ClaimsPrincipal user,
        GetVisitByIdHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(id, user, ct));

    private static async Task<IResult> CounterProposeAsync(
        Guid id,
        CounterProposeVisitRequest body,
        ClaimsPrincipal user,
        CounterProposeVisitHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new CounterProposeVisitCommand(id, body.Slots), user, ct);

        return Results.NoContent();
    }

    private static async Task<IResult> ScheduleAsync(
        Guid id,
        ScheduleVisitRequest body,
        ClaimsPrincipal user,
        ScheduleVisitHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new ScheduleVisitCommand(id, body.StartsAt), user, ct);

        return Results.NoContent();
    }

    private static async Task<IResult> CancelAsync(
        Guid id,
        CancelVisitRequest body,
        ClaimsPrincipal user,
        CancelVisitHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new CancelVisitCommand(id, body.Reason), user, ct);

        return Results.NoContent();
    }

    private static async Task<IResult> MarkCompletedAsync(
        Guid id,
        ClaimsPrincipal user,
        MarkVisitCompletedHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new MarkVisitCompletedCommand(id), user, ct);

        return Results.NoContent();
    }

    private static async Task<IResult> MarkNoShowAsync(
        Guid id,
        ClaimsPrincipal user,
        MarkVisitNoShowHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new MarkVisitNoShowCommand(id), user, ct);

        return Results.NoContent();
    }
}
