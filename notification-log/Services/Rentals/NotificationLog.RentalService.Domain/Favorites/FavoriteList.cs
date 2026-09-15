using Domain.Shared.Common;
using Domain.Shared.EventSourcing;
using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Favorites.Events;
using NotificationLog.RentalService.Domain.Favorites.ValueObjects;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Domain.Favorites;

public sealed class FavoriteList : AggregateRoot
{
    public const int MaxFavorites = 100;

    private readonly List<Guid> _listingIds = [];

    private FavoriteList() { }

    public Guid UserId { get; private set; }
    public IReadOnlyList<Guid> ListingIds => _listingIds.AsReadOnly();

    public static FavoriteList StartFor(Guid userId)
    {
        Guard.RequireId(userId, "El usuario es obligatorio.");

        var favorites = new FavoriteList();
        favorites.Raise(new FavoriteListStarted(FavoriteListId.For(userId), userId));

        return favorites;
    }

    public void Add(Guid listingId)
    {
        Guard.RequireId(listingId, "La publicación es obligatoria.");

        if (_listingIds.Contains(listingId))
            return;

        if (_listingIds.Count >= MaxFavorites)
            throw new DomainException($"No se pueden tener más de {MaxFavorites} favoritos.");

        Raise(new ListingFavorited(listingId));
    }

    public void Remove(Guid listingId)
    {
        if (!_listingIds.Contains(listingId))
            return;

        Raise(new ListingUnfavorited(listingId));
    }

    public override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case FavoriteListStarted e:
                Id = e.FavoriteListId;
                UserId = e.UserId;
                break;
            case ListingFavorited e: _listingIds.Add(e.ListingId); break;
            case ListingUnfavorited e: _listingIds.Remove(e.ListingId); break;

            default:
                throw new DomainException(
                    $"El evento '{domainEvent.GetType().Name}' no pertenece al agregado FavoriteList.");
        }
    }
}
