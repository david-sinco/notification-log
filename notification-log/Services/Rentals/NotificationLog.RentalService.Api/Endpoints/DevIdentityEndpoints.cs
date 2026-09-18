using Marten;
using NotificationLog.RentalService.Api.Contracts.Dev;
using NotificationLog.RentalService.Infrastructure.IdentityReplica;

namespace NotificationLog.RentalService.Api.Endpoints;

public static class DevIdentityEndpoints
{
    public static IEndpointRouteBuilder MapDevIdentity(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dev/identity").WithTags("Dev");

        group.MapGet("/", GetAsync)
            .WithName("DevGetIdentity")
            .WithSummary("Solo desarrollo: lista el contenido de la réplica de Identity")
            .Produces<DevIdentityResponse>();

        group.MapPut("/people/{personId:guid}", SetPersonVerificationAsync)
            .WithName("DevSetPersonVerification")
            .WithSummary("Solo desarrollo: registra la verificación de una persona en la réplica de Identity")
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    private static async Task<IResult> GetAsync(IQuerySession session, CancellationToken ct)
        => Results.Ok(new DevIdentityResponse(await session.Query<PersonVerificationDocument>().ToListAsync(ct)));

    private static async Task<IResult> SetPersonVerificationAsync(
        Guid personId,
        DevPersonVerificationRequest body,
        IDocumentSession session,
        CancellationToken ct)
    {
        session.Store(new PersonVerificationDocument
        {
            Id = personId,
            UserId = body.UserId,
            IsPhoneVerified = body.IsPhoneVerified,
            IsDocumentVerified = body.IsDocumentVerified
        });

        await session.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
