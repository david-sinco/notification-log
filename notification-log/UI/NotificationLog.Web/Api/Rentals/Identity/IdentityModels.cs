namespace NotificationLog.Web.Api.Rentals.Identity;

public sealed record PersonDto(Guid Id, Guid? UserId, bool IsPhoneVerified, bool IsDocumentVerified);

public sealed record IdentitySnapshotDto(IReadOnlyList<PersonDto> People);

public sealed record SetPersonRequest(Guid? UserId, bool IsPhoneVerified, bool IsDocumentVerified);
