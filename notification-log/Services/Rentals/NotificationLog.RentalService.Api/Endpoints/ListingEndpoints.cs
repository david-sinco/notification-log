using NotificationLog.RentalService.Api.Contracts.Listings;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Listings.Dtos;
using NotificationLog.RentalService.Application.Listings.Queries.GetListingById;
using NotificationLog.RentalService.Application.Listings.Queries.ListListings;
using NotificationLog.RentalService.Application.Listings.Commands.AssignAdvisor;
using NotificationLog.RentalService.Application.Listings.Commands.CancelReservation;
using NotificationLog.RentalService.Application.Listings.Commands.ChangeListingPrice;
using NotificationLog.RentalService.Application.Listings.Commands.CloseListing;
using NotificationLog.RentalService.Application.Listings.Commands.DraftListing;
using NotificationLog.RentalService.Application.Listings.Commands.ExtendReservation;
using NotificationLog.RentalService.Application.Listings.Commands.RenewListing;
using NotificationLog.RentalService.Application.Listings.Commands.ReportListing;
using NotificationLog.RentalService.Application.Listings.Commands.SetListingAvailability;
using NotificationLog.RentalService.Application.Listings.Commands.SubmitListingForReview;
using NotificationLog.RentalService.Application.Listings.Commands.UpdateListingDetails;
using NotificationLog.RentalService.Application.Listings.Commands.UpdateListingPhotos;
using NotificationLog.RentalService.Application.Listings.Commands.WithdrawListing;

namespace NotificationLog.RentalService.Api.Endpoints;

public static class ListingEndpoints
{
    public static IEndpointRouteBuilder MapListings(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/listings").WithTags("Listings");

        group.MapPost("/", DraftAsync)
            .WithName("DraftListing")
            .WithSummary("Crea una publicación en borrador")
            .Produces<CreatedListingResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/", ListAsync)
            .WithName("ListListings")
            .WithSummary("Lista publicaciones con filtros y paginación")
            .Produces<PagedResult<ListingSummaryDto>>();

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetListingById")
            .WithSummary("Obtiene una publicación por su identificador")
            .Produces<ListingDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/details", UpdateDetailsAsync)
            .WithName("UpdateListingDetails")
            .WithSummary("Actualiza los datos del inmueble, la ubicación y la descripción")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/photos", UpdatePhotosAsync)
            .WithName("UpdateListingPhotos")
            .WithSummary("Reemplaza las fotos de la publicación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPatch("/{id:guid}/price", ChangePriceAsync)
            .WithName("ChangeListingPrice")
            .WithSummary("Cambia el precio de la publicación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/submit", SubmitForReviewAsync)
            .WithName("SubmitListingForReview")
            .WithSummary("Envía la publicación a revisión")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPatch("/{id:guid}/availability", SetAvailabilityAsync)
            .WithName("SetListingAvailability")
            .WithSummary("Pausa o reanuda la publicación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/renew", RenewAsync)
            .WithName("RenewListing")
            .WithSummary("Renueva la vigencia de la publicación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/advisor", AssignAdvisorAsync)
            .WithName("AssignAdvisor")
            .WithSummary("Asigna o quita el asesor de la publicación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/withdraw", WithdrawAsync)
            .WithName("WithdrawListing")
            .WithSummary("Retira la publicación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/reports", ReportAsync)
            .WithName("ReportListing")
            .WithSummary("Reporta la publicación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/reservation/extend", ExtendReservationAsync)
            .WithName("ExtendReservation")
            .WithSummary("Amplía la reserva de la publicación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/reservation/cancel", CancelReservationAsync)
            .WithName("CancelReservation")
            .WithSummary("Cancela la reserva y la publicación vuelve a estar disponible")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/close", CloseAsync)
            .WithName("CloseListing")
            .WithSummary("Cierra la publicación con la firma del contrato")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return app;
    }

    private static async Task<IResult> DraftAsync(
        DraftListingRequest body,
        DraftListingHandler handler,
        CancellationToken ct)
    {
        var id = await handler.HandleAsync(
            new DraftListingCommand(body.ActorId, body.PublisherId, body.AdvisorId, body.Operation), ct);

        return Results.Created($"/api/listings/{id}", new CreatedListingResponse(id));
    }

    private static async Task<IResult> ListAsync(
        [AsParameters] ListListingsQuery query,
        ListListingsHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(query, ct));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        GetListingByIdHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(new GetListingByIdQuery(id), ct));

    private static async Task<IResult> UpdateDetailsAsync(
        Guid id,
        UpdateListingDetailsRequest body,
        UpdateListingDetailsHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new UpdateListingDetailsCommand(
            body.ActorId,
            id,
            body.Type,
            body.Area,
            body.Bedrooms,
            body.Bathrooms,
            body.ParkingSpots,
            body.Stratum,
            body.Floor,
            body.HasElevator,
            body.AdministrationFee,
            body.City,
            body.Neighborhood,
            body.Address,
            body.Description), ct);

        return Results.NoContent();
    }

    private static async Task<IResult> UpdatePhotosAsync(
        Guid id,
        UpdateListingPhotosRequest body,
        UpdateListingPhotosHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new UpdateListingPhotosCommand(body.ActorId, id, body.Photos), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ChangePriceAsync(
        Guid id,
        ChangeListingPriceRequest body,
        ChangeListingPriceHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new ChangeListingPriceCommand(body.ActorId, id, body.Price), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SubmitForReviewAsync(
        Guid id,
        SubmitListingForReviewRequest body,
        SubmitListingForReviewHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new SubmitListingForReviewCommand(body.ActorId, id), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SetAvailabilityAsync(
        Guid id,
        SetListingAvailabilityRequest body,
        SetListingAvailabilityHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new SetListingAvailabilityCommand(body.ActorId, id, body.IsAvailable), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RenewAsync(
        Guid id,
        RenewListingRequest body,
        RenewListingHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new RenewListingCommand(body.ActorId, id), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> AssignAdvisorAsync(
        Guid id,
        AssignAdvisorRequest body,
        AssignAdvisorHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new AssignAdvisorCommand(body.ActorId, id, body.AdvisorId), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> WithdrawAsync(
        Guid id,
        WithdrawListingRequest body,
        WithdrawListingHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new WithdrawListingCommand(body.ActorId, id, body.Reason, ByModerator: false), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ReportAsync(
        Guid id,
        ReportListingRequest body,
        ReportListingHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new ReportListingCommand(body.ReporterId, id, body.Reason), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ExtendReservationAsync(
        Guid id,
        ExtendReservationRequest body,
        ExtendReservationHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new ExtendReservationCommand(body.ActorId, id), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> CancelReservationAsync(
        Guid id,
        CancelReservationRequest body,
        CancelReservationHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new CancelReservationCommand(body.ActorId, id, body.Reason), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> CloseAsync(
        Guid id,
        CloseListingRequest body,
        CloseListingHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new CloseListingCommand(body.ActorId, id, body.FinalPrice, body.SignedOn), ct);
        return Results.NoContent();
    }
}
