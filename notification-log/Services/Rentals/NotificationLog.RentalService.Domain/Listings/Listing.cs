using Domain.Shared.Common;
using Domain.Shared.EventSourcing;
using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Listings.Events;
using NotificationLog.RentalService.Domain.Listings.ValueObjects;
using NotificationLog.RentalService.Domain.Shared;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Domain.Listings;

public sealed class Listing : AggregateRoot
{
    private readonly List<Photo> _photos = [];

    private Listing() { }

    public Guid OwnerId { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Operation Operation { get; private set; }
    public PropertyDetails? Details { get; private set; }
    public Location? Location { get; private set; }
    public ListingDescription? Description { get; private set; }
    public Money? Price { get; private set; }
    public ListingStatus Status { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public DateTimeOffset? LastPriceChangeAt { get; private set; }
    public IReadOnlyList<Photo> Photos => _photos.AsReadOnly();

    private bool IsVisible => Status is ListingStatus.Published or ListingStatus.Paused;

    private bool HasBeenPublished =>
        Status is ListingStatus.Published or ListingStatus.Paused or ListingStatus.Expired;

    public static Listing Draft(Guid id, Guid ownerId, Guid createdBy, Operation operation)
    {
        Guard.RequireId(id, "El identificador de la publicación es obligatorio.");
        Guard.RequireId(ownerId, "El propietario de la publicación es obligatorio.");
        Guard.RequireId(createdBy, "El usuario que crea la publicación es obligatorio.");

        if (!Enum.IsDefined(operation))
            throw new DomainException("La operación de la publicación no es válida.");

        var listing = new Listing();

        listing.Raise(new ListingDrafted(id, ownerId, createdBy, operation));

        return listing;
    }

    public void UpdateDetails(PropertyDetails details, Location location, ListingDescription description)
    {
        ArgumentNullException.ThrowIfNull(details);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(description);
        EnsureEditable();

        if (details == Details && location == Location && description == Description)
            return;

        Raise(new ListingDetailsUpdated(
            details.Type,
            details.Area,
            details.Bedrooms,
            details.Bathrooms,
            details.ParkingSpots,
            details.Stratum.Value,
            details.Floor,
            details.HasElevator,
            details.AdministrationFee.Amount,
            location.City,
            location.Neighborhood,
            location.Address,
            description.Value));

        SendBackToReviewIfVisible();
    }

    public void UpdatePhotos(IReadOnlyList<Photo> photos)
    {
        ArgumentNullException.ThrowIfNull(photos);
        EnsureEditable();

        if (photos.Any(photo => photo is null))
            throw new ArgumentException("La lista de fotos contiene elementos nulos.", nameof(photos));

        if (photos.Count > ListingPolicy.MaxPhotos)
            throw new DomainException($"Una publicación no puede tener más de {ListingPolicy.MaxPhotos} fotos.");

        if (photos.Select(photo => photo.Reference).Distinct().Count() != photos.Count)
            throw new DomainException("La publicación tiene fotos repetidas.");

        if (photos.SequenceEqual(_photos))
            return;

        Raise(new ListingPhotosUpdated(photos.Select(photo => photo.Reference).ToList()));

        SendBackToReviewIfVisible();
    }

    public void ChangePrice(Money price, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(price);

        if (Status is not (ListingStatus.Draft or ListingStatus.InReview) && !HasBeenPublished)
            throw new DomainException("El precio de esta publicación ya no se puede cambiar.");

        var minimum = Money.Create(ListingPolicy.MinimumPriceFor(Operation));

        if (price.Amount < minimum.Amount)
            throw new DomainException(Operation == Operation.Rent
                ? $"El canon mínimo de arriendo es {minimum}."
                : $"El precio mínimo de venta es {minimum}.");

        if (price == Price)
            return;

        if (HasBeenPublished
            && LastPriceChangeAt is { } lastChange
            && now - lastChange < ListingPolicy.PriceChangeCooldown)
            throw new DomainException("El precio solo se puede cambiar una vez cada 24 horas.");

        Raise(new ListingPriceChanged(Price?.Amount ?? 0, price.Amount), now);
    }

    public void SubmitForReview()
    {
        if (Status != ListingStatus.Draft)
            throw new DomainException("Solo se puede enviar a revisión una publicación en borrador.");

        if (Details is null || Location is null || Description is null)
            throw new DomainException("Faltan los datos del inmueble, la ubicación o la descripción.");

        if (Price is null)
            throw new DomainException("Falta el precio de la publicación.");

        if (_photos.Count < ListingPolicy.MinPhotos)
            throw new DomainException($"La publicación necesita al menos {ListingPolicy.MinPhotos} fotos.");

        Raise(new ListingSubmittedForReview());
    }

    public void Approve(Guid moderatorId, DateTimeOffset now)
    {
        Guard.RequireId(moderatorId, "El moderador es obligatorio.");

        if (Status != ListingStatus.InReview)
            throw new DomainException("Solo se puede aprobar una publicación en revisión.");

        Raise(new ListingApproved(moderatorId, ListingPolicy.ExpiryFrom(now)), now);
    }

    public void Reject(Guid moderatorId, IReadOnlyCollection<RejectionReason> reasons)
    {
        Guard.RequireId(moderatorId, "El moderador es obligatorio.");
        ArgumentNullException.ThrowIfNull(reasons);

        if (Status != ListingStatus.InReview)
            throw new DomainException("Solo se puede rechazar una publicación en revisión.");

        if (reasons.Count == 0)
            throw new DomainException("Hay que indicar al menos un motivo de rechazo.");

        if (reasons.Any(reason => !Enum.IsDefined(reason)))
            throw new DomainException("Uno de los motivos de rechazo no es válido.");

        Raise(new ListingRejected(moderatorId, reasons.Distinct().ToList()));
    }

    public void Pause()
    {
        if (Status == ListingStatus.Paused)
            return;

        if (Status != ListingStatus.Published)
            throw new DomainException("Solo se puede pausar una publicación publicada.");

        Raise(new ListingPaused());
    }

    public void Resume()
    {
        if (Status == ListingStatus.Published)
            return;

        if (Status != ListingStatus.Paused)
            throw new DomainException("Solo se puede reanudar una publicación pausada.");

        Raise(new ListingResumed());
    }

    public void Renew(DateTimeOffset now)
    {
        var withinWindow = Status == ListingStatus.Published
            && ExpiresAt is { } expiresAt
            && expiresAt - now <= ListingPolicy.RenewalWindow;

        if (Status != ListingStatus.Expired && !withinWindow)
            throw new DomainException(
                $"Solo se puede renovar una publicación vencida o a la que le queden {ListingPolicy.RenewalWindow.Days} días o menos.");

        Raise(new ListingRenewed(ListingPolicy.ExpiryFrom(now)), now);
    }

    public void Expire(DateTimeOffset now)
    {
        if (!IsVisible || ExpiresAt is not { } expiresAt || expiresAt > now)
            return;

        Raise(new ListingExpired(), now);
    }

    public void Close(Money finalPrice, DateOnly signedOn, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(finalPrice);

        if (!IsVisible)
            throw new DomainException("Solo se puede cerrar una publicación publicada o pausada.");

        if (finalPrice.Amount == 0)
            throw new DomainException("El valor final debe ser mayor que cero.");

        if (signedOn > DateOnly.FromDateTime(now.UtcDateTime))
            throw new DomainException("La fecha de firma no puede estar en el futuro.");

        Raise(new ListingClosed(finalPrice.Amount, signedOn), now);
    }

    public void Withdraw(string reason)
    {
        if (Status == ListingStatus.Withdrawn)
            return;

        if (Status == ListingStatus.Closed)
            throw new DomainException("Una publicación cerrada no se puede retirar.");

        Raise(new ListingWithdrawn(Guard.RequireReason(reason, ListingPolicy.MaxReasonLength)));
    }

    public void Suspend(string reason)
    {
        if (Status == ListingStatus.Suspended)
            return;

        if (!IsVisible)
            throw new DomainException("Solo se puede suspender una publicación publicada o pausada.");

        Raise(new ListingSuspended(Guard.RequireReason(reason, ListingPolicy.MaxReasonLength)));
    }

    public void Reinstate(Guid moderatorId, DateTimeOffset now)
    {
        Guard.RequireId(moderatorId, "El moderador es obligatorio.");

        if (Status != ListingStatus.Suspended)
            throw new DomainException("La publicación no está suspendida.");

        Raise(new ListingReinstated(moderatorId), now);
        Expire(now);
    }

    public override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case ListingDrafted e: When(e); break;
            case ListingDetailsUpdated e: When(e); break;
            case ListingPhotosUpdated e: When(e); break;
            case ListingPriceChanged e: When(e); break;
            case ListingSubmittedForReview: Status = ListingStatus.InReview; break;
            case ListingApproved e: MarkPublished(e.ExpiresAt); break;
            case ListingRejected: Status = ListingStatus.Draft; break;
            case ListingPaused: Status = ListingStatus.Paused; break;
            case ListingResumed: Status = ListingStatus.Published; break;
            case ListingRenewed e: MarkPublished(e.ExpiresAt); break;
            case ListingExpired: Status = ListingStatus.Expired; break;
            case ListingClosed: Status = ListingStatus.Closed; break;
            case ListingWithdrawn: Status = ListingStatus.Withdrawn; break;
            case ListingSuspended: Status = ListingStatus.Suspended; break;
            case ListingReinstated: Status = ListingStatus.Published; break;

            default:
                throw new DomainException(
                    $"El evento '{domainEvent.GetType().Name}' no pertenece al agregado Listing.");
        }
    }

    private void When(ListingDrafted e)
    {
        Id = e.ListingId;
        OwnerId = e.OwnerId;
        CreatedBy = e.CreatedBy;
        Operation = e.Operation;
        Status = ListingStatus.Draft;
    }

    private void When(ListingDetailsUpdated e)
    {
        Details = PropertyDetails.FromStorage(
            e.Type,
            e.Area,
            e.Bedrooms,
            e.Bathrooms,
            e.ParkingSpots,
            Stratum.FromStorage(e.Stratum),
            e.Floor,
            e.HasElevator,
            Money.FromStorage(e.AdministrationFee));

        Location = Location.FromStorage(e.City, e.Neighborhood, e.Address);
        Description = ListingDescription.FromStorage(e.Description);
    }

    private void When(ListingPhotosUpdated e)
    {
        _photos.Clear();
        _photos.AddRange(e.Photos.Select(Photo.FromStorage));
    }

    private void When(ListingPriceChanged e)
    {
        if (HasBeenPublished)
            LastPriceChangeAt = OccurredAt(e);

        Price = Money.FromStorage(e.NewPrice);
    }

    private void MarkPublished(DateTimeOffset expiresAt)
    {
        Status = ListingStatus.Published;
        ExpiresAt = expiresAt;
    }

    private void Raise(DomainEvent domainEvent, DateTimeOffset now)
        => Raise(domainEvent with { OccurredOn = now.UtcDateTime });

    private void SendBackToReviewIfVisible()
    {
        if (IsVisible)
            Raise(new ListingSubmittedForReview());
    }

    private void EnsureEditable()
    {
        if (Status is not (ListingStatus.Draft or ListingStatus.InReview or ListingStatus.Published or ListingStatus.Paused))
            throw new DomainException(
                "Solo se puede editar una publicación en borrador, en revisión, publicada o pausada.");
    }

    private static DateTimeOffset OccurredAt(DomainEvent domainEvent)
        => new(DateTime.SpecifyKind(domainEvent.OccurredOn, DateTimeKind.Utc));
}
