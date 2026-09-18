namespace NotificationLog.Web.Api.Rentals.Identity;

public sealed record PersonDto(Guid Id, Guid? UserId, bool IsPhoneVerified, bool IsDocumentVerified);

public sealed record AdvisorDto(Guid Id, bool IsActive, IReadOnlyList<string> ServiceCities, int Capacity);

public sealed record AlertsConsentDto(Guid Id, bool IsGranted);

public sealed record IdentitySnapshotDto(
    IReadOnlyList<PersonDto> People,
    IReadOnlyList<AdvisorDto> Advisors,
    IReadOnlyList<AlertsConsentDto> AlertsConsents);

public sealed record SetPersonRequest(Guid? UserId, bool IsPhoneVerified, bool IsDocumentVerified);

public sealed record SetAdvisorRequest(bool IsActive, IReadOnlyList<string> ServiceCities, int Capacity);

public sealed record SetAlertsConsentRequest(bool IsGranted);
