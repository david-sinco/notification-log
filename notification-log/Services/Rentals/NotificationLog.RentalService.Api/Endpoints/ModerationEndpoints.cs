using System.Security.Claims;
using NotificationLog.RentalService.Api.Contracts.Listings;
using NotificationLog.RentalService.Api.Contracts.Moderation;
using NotificationLog.RentalService.Application.Listings.Commands.ReinstateListing;
using NotificationLog.RentalService.Application.Listings.Commands.ReviewListing;
using NotificationLog.RentalService.Application.Listings.Commands.SuspendListing;
using NotificationLog.RentalService.Application.Listings.Commands.WithdrawListing;
using API.Shared.Extensions;

namespace NotificationLog.RentalService.Api.Endpoints;

public static class ModerationEndpoints
{
    public static IEndpointRouteBuilder MapModeration(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/moderation/listings")
            .WithTags("Moderation")
            .RequireAuthorization(AuthorizationExtensions.ModeracionPolicy);

        group.MapPost("/{id:guid}/review", ReviewAsync)
            .WithName("ReviewListing")
            .WithSummary("Aprueba o rechaza una publicación en revisión")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/suspend", SuspendAsync)
            .WithName("SuspendListing")
            .WithSummary("Suspende una publicación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/reinstate", ReinstateAsync)
            .WithName("ReinstateListing")
            .WithSummary("Levanta la suspensión de una publicación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/withdraw", WithdrawAsync)
            .WithName("WithdrawListingByModerator")
            .WithSummary("Retira una publicación por decisión de moderación")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return app;
    }

    private static async Task<IResult> ReviewAsync(
        Guid id,
        ReviewListingRequest body,
        ClaimsPrincipal user,
        ReviewListingHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new ReviewListingCommand(id, body.Approve, body.Reasons), user, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SuspendAsync(
        Guid id,
        SuspendListingRequest body,
        ClaimsPrincipal user,
        SuspendListingHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new SuspendListingCommand(id, body.Reason), user, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ReinstateAsync(
        Guid id,
        ClaimsPrincipal user,
        ReinstateListingHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new ReinstateListingCommand(id), user, ct);
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
}
