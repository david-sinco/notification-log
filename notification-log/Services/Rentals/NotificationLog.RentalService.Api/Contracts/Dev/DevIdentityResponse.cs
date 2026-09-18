using NotificationLog.RentalService.Infrastructure.IdentityReplica;

namespace NotificationLog.RentalService.Api.Contracts.Dev;

public sealed record DevIdentityResponse(
    IReadOnlyList<PersonVerificationDocument> People,
    IReadOnlyList<AdvisorDocument> Advisors);
