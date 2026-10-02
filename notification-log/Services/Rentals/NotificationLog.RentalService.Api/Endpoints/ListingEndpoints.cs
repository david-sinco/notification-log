using System.Security.Claims;
using Application.Shared.Pagination;
using NotificationLog.RentalService.Api.Contracts.Listings;
using NotificationLog.RentalService.Application.Listings.Queries;
using NotificationLog.RentalService.Application.Listings.Queries.Dtos;
using NotificationLog.RentalService.Application.Listings.Queries.Filters;
using Microsoft.AspNetCore.Mvc;
using NotificationLog.RentalService.Application.Listings.Commands.AddListingPhoto;
using NotificationLog.RentalService.Application.Listings.Commands.RemoveListingPhoto;
using NotificationLog.RentalService.Application.Listings.Commands.ReorderListingPhotos;
using NotificationLog.RentalService.Application.Listings.Commands.ChangeListingPrice;
using NotificationLog.RentalService.Application.Listings.Commands.CloseListing;
using NotificationLog.RentalService.Application.Listings.Commands.DraftListing;
using NotificationLog.RentalService.Application.Listings.Commands.PauseListing;
using NotificationLog.RentalService.Application.Listings.Commands.RenewListing;
using NotificationLog.RentalService.Application.Listings.Commands.ResumeListing;
using NotificationLog.RentalService.Application.Listings.Commands.SubmitListingForReview;
using NotificationLog.RentalService.Application.Listings.Commands.UpdateListingDetails;
using NotificationLog.RentalService.Application.Listings.Commands.WithdrawListing;
using API.Shared.Extensions;

namespace NotificationLog.RentalService.Api.Endpoints;

public static class ListingEndpoints
{
    private const long MaxPhotoBytes = 10 * 1024 * 1024;

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

        group.MapPost("/{id:guid}/photos", AddPhotoAsync)
            .WithName("AddListingPhoto")
            .WithSummary("Sube una foto (JPEG, PNG o WebP) y la añade a la publicación")
            .WithMetadata(new RequestSizeLimitAttribute(MaxPhotoBytes))
            .DisableAntiforgery()
            .Produces<AddedListingPhotoResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{id:guid}/photos/{fileName}", RemovePhotoAsync)
            .WithName("RemoveListingPhoto")
            .WithSummary("Quita una foto de la publicación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/photos/order", ReorderPhotosAsync)
            .WithName("ReorderListingPhotos")
            .WithSummary("Cambia el orden de las fotos de la publicación")
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

        group.MapPost("/{id:guid}/pause", PauseAsync)
            .WithName("PauseListing")
            .WithSummary("Pausa la publicación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/resume", ResumeAsync)
            .WithName("ResumeListing")
            .WithSummary("Reanuda la publicación")
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
        [AsParameters] ListingFilter filter,
        [AsParameters] PageRequest paging,
        ListListingsHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(filter, paging, ct));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        GetListingByIdHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(id, ct));

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

    private static async Task<IResult> AddPhotoAsync(
        Guid id,
        IFormFile file,
        ClaimsPrincipal user,
        AddListingPhotoHandler handler,
        CancellationToken ct)
    {
        await using var content = file.OpenReadStream();
        var fileName = await handler.HandleAsync(new AddListingPhotoCommand(id, content), user, ct);

        return Results.Created($"/api/listings/{id}", new AddedListingPhotoResponse(fileName));
    }

    private static async Task<IResult> RemovePhotoAsync(
        Guid id,
        string fileName,
        ClaimsPrincipal user,
        RemoveListingPhotoHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new RemoveListingPhotoCommand(id, fileName), user, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ReorderPhotosAsync(
        Guid id,
        ReorderListingPhotosRequest body,
        ClaimsPrincipal user,
        ReorderListingPhotosHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new ReorderListingPhotosCommand(id, body.FileNames), user, ct);
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

    private static async Task<IResult> PauseAsync(
        Guid id,
        ClaimsPrincipal user,
        PauseListingHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new PauseListingCommand(id), user, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ResumeAsync(
        Guid id,
        ClaimsPrincipal user,
        ResumeListingHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new ResumeListingCommand(id), user, ct);
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
