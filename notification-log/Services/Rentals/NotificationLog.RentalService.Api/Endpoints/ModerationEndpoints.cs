using System.Security.Claims;
using NotificationLog.RentalService.Api.Contracts.Listings;
using NotificationLog.RentalService.Api.Contracts.Moderation;
using NotificationLog.RentalService.Application.Listings.Commands.ApproveListing;
using NotificationLog.RentalService.Application.Listings.Commands.ReinstateListing;
using NotificationLog.RentalService.Application.Listings.Commands.RejectListing;
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

        group.MapPost("/{id:guid}/approve", ApproveAsync)
            .WithName("ApproveListing")
            .WithSummary("Aprueba una publicación en revisión")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/reject", RejectAsync)
            .WithName("RejectListing")
            .WithSummary("Rechaza una publicación en revisión")
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

    private static async Task<IResult> ApproveAsync(
        Guid id,
        ClaimsPrincipal user,
        ApproveListingHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new ApproveListingCommand(id), user, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RejectAsync(
        Guid id,
        RejectListingRequest body,
        ClaimsPrincipal user,
        RejectListingHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new RejectListingCommand(id, body.Reasons), user, ct);
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
