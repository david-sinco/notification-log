using NotificationLog.RentalService.Api.Contracts.SavedSearches;
using NotificationLog.RentalService.Application.SavedSearches.Commands;
using NotificationLog.RentalService.Application.SavedSearches.Commands.CreateSavedSearch;
using NotificationLog.RentalService.Application.SavedSearches.Commands.DeleteSavedSearch;
using NotificationLog.RentalService.Application.SavedSearches.Commands.UpdateSavedSearch;
using NotificationLog.RentalService.Application.SavedSearches.Queries.GetSavedSearches;

namespace NotificationLog.RentalService.Api.Endpoints;

public static class SavedSearchEndpoints
{
    public static IEndpointRouteBuilder MapSavedSearches(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users/{userId:guid}/saved-searches").WithTags("SavedSearches");

        group.MapGet("/", ListAsync)
            .WithName("GetSavedSearches")
            .WithSummary("Lista las búsquedas guardadas del usuario")
            .Produces<IReadOnlyList<SavedSearchDto>>();

        group.MapPost("/", CreateAsync)
            .WithName("CreateSavedSearch")
            .WithSummary("Guarda una búsqueda con su frecuencia de alertas")
            .Produces<CreatedSavedSearchResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{searchId:guid}", UpdateAsync)
            .WithName("UpdateSavedSearch")
            .WithSummary("Actualiza una búsqueda guardada")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{searchId:guid}", DeleteAsync)
            .WithName("DeleteSavedSearch")
            .WithSummary("Elimina una búsqueda guardada")
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    private static async Task<IResult> ListAsync(
        Guid userId,
        GetSavedSearchesHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(new GetSavedSearchesQuery(userId), ct));

    private static async Task<IResult> CreateAsync(
        Guid userId,
        CreateSavedSearchRequest body,
        CreateSavedSearchHandler handler,
        CancellationToken ct)
    {
        var id = await handler.HandleAsync(
            new CreateSavedSearchCommand(userId, body.Name, ToInput(body.Criteria), body.Frequency), ct);

        return Results.Created($"/api/users/{userId}/saved-searches/{id}", new CreatedSavedSearchResponse(id));
    }

    private static async Task<IResult> UpdateAsync(
        Guid userId,
        Guid searchId,
        UpdateSavedSearchRequest body,
        UpdateSavedSearchHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(
            new UpdateSavedSearchCommand(userId, searchId, body.Name, ToInput(body.Criteria), body.Frequency), ct);

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAsync(
        Guid userId,
        Guid searchId,
        DeleteSavedSearchHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new DeleteSavedSearchCommand(userId, searchId), ct);
        return Results.NoContent();
    }

    private static SearchCriteriaInput ToInput(SearchCriteriaRequest criteria) =>
        new(
            criteria.Operation,
            criteria.City,
            criteria.Neighborhoods,
            criteria.MinPrice,
            criteria.MaxPrice,
            criteria.MinBedrooms,
            criteria.MinStratum,
            criteria.MaxStratum,
            criteria.MinArea);
}
