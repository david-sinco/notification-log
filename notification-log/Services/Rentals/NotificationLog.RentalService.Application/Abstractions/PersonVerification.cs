namespace NotificationLog.RentalService.Application.Abstractions;

public sealed record PersonVerification(Guid PersonId, Guid? UserId, bool IsPhoneVerified, bool IsDocumentVerified);
