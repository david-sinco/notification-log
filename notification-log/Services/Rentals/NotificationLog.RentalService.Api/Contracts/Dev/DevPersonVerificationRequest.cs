namespace NotificationLog.RentalService.Api.Contracts.Dev;

public sealed record DevPersonVerificationRequest(Guid? UserId, bool IsPhoneVerified, bool IsDocumentVerified);
