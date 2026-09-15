using JasperFx.Events;
using Marten.Events.Aggregation;
using NotificationLog.RentalService.Domain.Favorites.Events;

namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

public sealed class FavoriteListDecisionProjection : SingleStreamProjection<FavoriteListDecision, Guid>
{
    public override FavoriteListDecision? Evolve(FavoriteListDecision? snapshot, Guid id, IEvent e)
    {
        if (e.Data is FavoriteListStarted started)
            return new FavoriteListDecision { Id = id, UserId = started.UserId };

        switch (snapshot, e.Data)
        {
            case (not null, ListingFavorited f): snapshot.ListingIds.Add(f.ListingId); break;
            case (not null, ListingUnfavorited u): snapshot.ListingIds.Remove(u.ListingId); break;
        }

        return snapshot;
    }
}
