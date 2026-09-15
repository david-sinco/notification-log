namespace NotificationLog.RentalService.Infrastructure.IdentityReplica;

public sealed class PersonVerificationDocument
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool IsDocumentVerified { get; set; }
}
