using Application.Shared.Common;
using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Listings.Queries;
using NotificationLog.RentalService.Application.Listings.Queries.Dtos;
using NotificationLog.RentalService.Application.Listings.Queries.Filters;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Api.Endpoints;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalog(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/catalog")
            .WithTags("Catalog")
            .AllowAnonymous();

        group.MapGet("/", ListAsync)
            .WithName("ListPublishedListings")
            .WithSummary("Lista las publicaciones publicadas, sin iniciar sesión")
            .Produces<PagedResult<ListingSummaryDto>>();

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetPublishedListingById")
            .WithSummary("Obtiene una publicación publicada, sin iniciar sesión")
            .Produces<ListingDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListAsync(
        string? search,
        Operation? operation,
        [AsParameters] PageRequest paging,
        ListListingsHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(
            new ListingFilter(search, ListingStatus.Published, operation), paging, ct));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        GetListingByIdHandler handler,
        CancellationToken ct)
    {
        var listing = await handler.HandleAsync(id, ct);

        return listing.Status == nameof(ListingStatus.Published)
            ? Results.Ok(listing)
            : throw new NotFoundException(nameof(Listing), id);
    }
}
