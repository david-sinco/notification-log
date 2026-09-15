namespace NotificationLog.RentalService.Infrastructure.IdentityReplica;

public sealed class AdvisorDocument
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public List<string> ServiceCities { get; set; } = [];
    public int Capacity { get; set; }
}
