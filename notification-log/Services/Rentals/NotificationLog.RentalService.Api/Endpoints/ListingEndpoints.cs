using System.Security.Claims;
using NotificationLog.RentalService.Api.Authorization;
using NotificationLog.RentalService.Api.Contracts.Listings;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Listings.Dtos;
using NotificationLog.RentalService.Application.Listings.Queries.GetListingById;
using NotificationLog.RentalService.Application.Listings.Queries.ListListings;
using NotificationLog.RentalService.Application.Listings.Commands.ChangeListingPrice;
using NotificationLog.RentalService.Application.Listings.Commands.CloseListing;
using NotificationLog.RentalService.Application.Listings.Commands.DraftListing;
using NotificationLog.RentalService.Application.Listings.Commands.RenewListing;
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
        var group = app.MapGroup("/api/listings")
            .WithTags("Listings")
            .RequireAuthorization(AuthorizationExtensions.RentalsScopePolicy);

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

        group.MapPost("/{id:guid}/withdraw", WithdrawAsync)
            .WithName("WithdrawListing")
            .WithSummary("Retira la publicación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/close", CloseAsync)
            .WithName("CloseListing")
            .WithSummary("Cierra la publicación porque se arrendó o se vendió")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return app;
    }

    private static async Task<IResult> DraftAsync(
        DraftListingRequest body,
        ClaimsPrincipal user,
        DraftListingHandler handler,
        CancellationToken ct)
    {
        var id = await handler.HandleAsync(new DraftListingCommand(body.OwnerId, body.Operation), user, ct);

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
        ClaimsPrincipal user,
        UpdateListingDetailsHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new UpdateListingDetailsCommand(
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
            body.Description), user, ct);

        return Results.NoContent();
    }

    private static async Task<IResult> UpdatePhotosAsync(
        Guid id,
        UpdateListingPhotosRequest body,
        ClaimsPrincipal user,
        UpdateListingPhotosHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new UpdateListingPhotosCommand(id, body.Photos), user, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ChangePriceAsync(
        Guid id,
        ChangeListingPriceRequest body,
        ClaimsPrincipal user,
        ChangeListingPriceHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new ChangeListingPriceCommand(id, body.Price), user, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SubmitForReviewAsync(
        Guid id,
        ClaimsPrincipal user,
        SubmitListingForReviewHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new SubmitListingForReviewCommand(id), user, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SetAvailabilityAsync(
        Guid id,
        SetListingAvailabilityRequest body,
        ClaimsPrincipal user,
        SetListingAvailabilityHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new SetListingAvailabilityCommand(id, body.IsAvailable), user, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RenewAsync(
        Guid id,
        ClaimsPrincipal user,
        RenewListingHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new RenewListingCommand(id), user, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> WithdrawAsync(
        Guid id,
        WithdrawListingRequest body,
        ClaimsPrincipal user,
        WithdrawListingHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new WithdrawListingCommand(id, body.Reason), user, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> CloseAsync(
        Guid id,
        CloseListingRequest body,
        ClaimsPrincipal user,
        CloseListingHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new CloseListingCommand(id, body.FinalPrice, body.SignedOn), user, ct);
        return Results.NoContent();
    }
}
