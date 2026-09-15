namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

public sealed class SavedSearchListDecision
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<SavedSearchEntry> Searches { get; set; } = [];
}
