using Google.Protobuf.WellKnownTypes;
using Marten;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Api.Contracts.Dev;
using NotificationLog.RentalService.Infrastructure.IdentityReplica;
using Wolverine;

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

        group.MapPut("/advisors/{advisorId:guid}", SetAdvisorAsync)
            .WithName("DevSetAdvisor")
            .WithSummary("Solo desarrollo: registra un asesor en la réplica de Identity")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPut("/users/{userId:guid}/alerts-consent", SetAlertsConsentAsync)
            .WithName("DevSetAlertsConsent")
            .WithSummary("Solo desarrollo: registra el consentimiento de alertas en la réplica de Identity")
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    private static async Task<IResult> GetAsync(IQuerySession session, CancellationToken ct)
        => Results.Ok(new DevIdentityResponse(
            await session.Query<PersonVerificationDocument>().ToListAsync(ct),
            await session.Query<AdvisorDocument>().ToListAsync(ct),
            await session.Query<AlertsConsentDocument>().ToListAsync(ct)));

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

    private static async Task<IResult> SetAdvisorAsync(
        Guid advisorId,
        DevAdvisorRequest body,
        IMessageBus bus,
        CancellationToken ct)
    {
        var message = new AdvisorChanged
        {
            EventId = Guid.NewGuid().ToString(),
            OccurredAt = Timestamp.FromDateTime(DateTime.UtcNow),
            SchemaVersion = 1,
            AdvisorId = advisorId.ToString(),
            IsActive = body.IsActive,
            Capacity = body.Capacity
        };

        message.ServiceCities.Add(body.ServiceCities);

        await bus.InvokeAsync(message, ct);

        return Results.NoContent();
    }

    private static async Task<IResult> SetAlertsConsentAsync(
        Guid userId,
        DevAlertsConsentRequest body,
        IMessageBus bus,
        CancellationToken ct)
    {
        await bus.InvokeAsync(new AlertsConsentChanged
        {
            EventId = Guid.NewGuid().ToString(),
            OccurredAt = Timestamp.FromDateTime(DateTime.UtcNow),
            SchemaVersion = 1,
            UserId = userId.ToString(),
            IsGranted = body.IsGranted
        }, ct);

        return Results.NoContent();
    }
}
