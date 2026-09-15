using NotificationLog.RentalService.Application.Favorites.Commands.AddFavorite;
using NotificationLog.RentalService.Application.Favorites.Commands.RemoveFavorite;
using NotificationLog.RentalService.Application.Favorites.Queries.GetFavorites;
using NotificationLog.RentalService.Application.Listings.Queries.ListListings;

namespace NotificationLog.RentalService.Api.Endpoints;

public static class FavoriteEndpoints
{
    public static IEndpointRouteBuilder MapFavorites(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users/{userId:guid}/favorites").WithTags("Favorites");

        group.MapGet("/", ListAsync)
            .WithName("GetFavorites")
            .WithSummary("Lista las publicaciones favoritas del usuario")
            .Produces<IReadOnlyList<ListingSummaryDto>>();

        group.MapPut("/{listingId:guid}", AddAsync)
            .WithName("AddFavorite")
            .WithSummary("Agrega una publicación a los favoritos del usuario")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{listingId:guid}", RemoveAsync)
            .WithName("RemoveFavorite")
            .WithSummary("Quita una publicación de los favoritos del usuario")
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    private static async Task<IResult> ListAsync(
        Guid userId,
        GetFavoritesHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(new GetFavoritesQuery(userId), ct));

    private static async Task<IResult> AddAsync(
        Guid userId,
        Guid listingId,
        AddFavoriteHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new AddFavoriteCommand(userId, listingId), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveAsync(
        Guid userId,
        Guid listingId,
        RemoveFavoriteHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new RemoveFavoriteCommand(userId, listingId), ct);
        return Results.NoContent();
    }
}
