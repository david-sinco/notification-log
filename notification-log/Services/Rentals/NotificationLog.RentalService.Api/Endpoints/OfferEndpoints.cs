using NotificationLog.RentalService.Api.Contracts.Offers;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Offers.Dtos;
using NotificationLog.RentalService.Application.Offers.Queries.GetOfferById;
using NotificationLog.RentalService.Application.Offers.Queries.ListOffers;
using NotificationLog.RentalService.Application.Offers.Commands.AcceptOffer;
using NotificationLog.RentalService.Application.Offers.Commands.CounterOffer;
using NotificationLog.RentalService.Application.Offers.Commands.RejectOffer;
using NotificationLog.RentalService.Application.Offers.Commands.SubmitOffer;
using NotificationLog.RentalService.Application.Offers.Commands.WithdrawOffer;

namespace NotificationLog.RentalService.Api.Endpoints;

public static class OfferEndpoints
{
    public static IEndpointRouteBuilder MapOffers(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/offers").WithTags("Offers");

        group.MapPost("/", SubmitAsync)
            .WithName("SubmitOffer")
            .WithSummary("Hace una oferta por una publicación")
            .Produces<CreatedOfferResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/", ListAsync)
            .WithName("ListOffers")
            .WithSummary("Lista ofertas con filtros y paginación")
            .Produces<PagedResult<OfferSummaryDto>>();

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetOfferById")
            .WithSummary("Obtiene una oferta con su historial de negociación")
            .Produces<OfferDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/counter", CounterAsync)
            .WithName("CounterOffer")
            .WithSummary("Responde la oferta con una contraoferta")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/accept", AcceptAsync)
            .WithName("AcceptOffer")
            .WithSummary("Acepta la oferta y reserva la publicación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/reject", RejectAsync)
            .WithName("RejectOffer")
            .WithSummary("Rechaza la oferta")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/withdraw", WithdrawAsync)
            .WithName("WithdrawOffer")
            .WithSummary("Retira la oferta")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return app;
    }

    private static async Task<IResult> SubmitAsync(
        SubmitOfferRequest body,
        SubmitOfferHandler handler,
        CancellationToken ct)
    {
        var id = await handler.HandleAsync(new SubmitOfferCommand(
            body.OffererId,
            body.ListingId,
            body.Amount,
            body.RentStartDate,
            body.RentTermMonths), ct);

        return Results.Created($"/api/offers/{id}", new CreatedOfferResponse(id));
    }

    private static async Task<IResult> ListAsync(
        [AsParameters] ListOffersQuery query,
        ListOffersHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(query, ct));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        GetOfferByIdHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(new GetOfferByIdQuery(id), ct));

    private static async Task<IResult> CounterAsync(
        Guid id,
        CounterOfferRequest body,
        CounterOfferHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new CounterOfferCommand(body.ActorId, id, body.Amount), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> AcceptAsync(
        Guid id,
        AcceptOfferRequest body,
        AcceptOfferHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new AcceptOfferCommand(body.ActorId, id), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RejectAsync(
        Guid id,
        RejectOfferRequest body,
        RejectOfferHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new RejectOfferCommand(body.ActorId, id, body.Reason), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> WithdrawAsync(
        Guid id,
        WithdrawOfferRequest body,
        WithdrawOfferHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new WithdrawOfferCommand(body.ActorId, id), ct);
        return Results.NoContent();
    }
}
