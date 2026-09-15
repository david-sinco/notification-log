namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

public sealed class FavoriteListDecision
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<Guid> ListingIds { get; set; } = [];
}
