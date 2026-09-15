using Domain.Shared.Common;
using Domain.Shared.EventSourcing;
using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Offers.Events;
using NotificationLog.RentalService.Domain.Offers.ValueObjects;
using NotificationLog.RentalService.Domain.Shared;
using NotificationLog.RentalService.Domain.Offers.Enums;

namespace NotificationLog.RentalService.Domain.Offers;

public sealed class Offer : AggregateRoot
{
    private Offer() { }

    public Guid ListingId { get; private set; }
    public Guid OffererId { get; private set; }
    public Operation Operation { get; private set; }
    public Money ListedPrice { get; private set; } = null!;
    public Money Amount { get; private set; } = null!;
    public RentTerms? RentTerms { get; private set; }
    public OfferStatus Status { get; private set; }
    public OfferParty LastMoveBy { get; private set; }
    public int CounterOfferCount { get; private set; }
    public DateTimeOffset RespondBy { get; private set; }

    public bool IsOpen => Status is OfferStatus.AwaitingPublisher or OfferStatus.AwaitingOfferer;

    public static Offer Submit(
        Guid id,
        Guid listingId,
        Guid offererId,
        Operation operation,
        Money listedPrice,
        Money amount,
        RentTerms? rentTerms,
        DateTimeOffset now)
    {
        Guard.RequireId(id, "El identificador de la oferta es obligatorio.");
        Guard.RequireId(listingId, "La publicación de la oferta es obligatoria.");
        Guard.RequireId(offererId, "El interesado que hace la oferta es obligatorio.");
        ArgumentNullException.ThrowIfNull(listedPrice);
        ArgumentNullException.ThrowIfNull(amount);

        if (!Enum.IsDefined(operation))
            throw new DomainException("La operación de la oferta no es válida.");

        if (operation == Operation.Rent && rentTerms is null)
            throw new DomainException("Una oferta de arriendo necesita fecha de inicio y duración.");

        if (operation == Operation.Sale && rentTerms is not null)
            throw new DomainException("Una oferta de venta no lleva condiciones de arriendo.");

        EnsureAboveMinimum(amount, listedPrice);

        var offer = new Offer();

        offer.Raise(new OfferSubmitted(
            id,
            listingId,
            offererId,
            operation,
            listedPrice.Amount,
            amount.Amount,
            rentTerms?.StartDate,
            rentTerms?.Months,
            now + OfferPolicy.ResponseWindow));

        return offer;
    }

    public void Counter(OfferParty by, Money amount, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(amount);
        EnsureTurnOf(by, now);

        if (CounterOfferCount >= OfferPolicy.MaxCounterOffers)
            throw new DomainException($"Una oferta admite como máximo {OfferPolicy.MaxCounterOffers} contraofertas.");

        if (amount == Amount)
            throw new DomainException("La contraoferta tiene que cambiar el monto.");

        EnsureAboveMinimum(amount, ListedPrice);

        Raise(new OfferCountered(by, amount.Amount, now + OfferPolicy.ResponseWindow));
    }

    public void Accept(OfferParty by, DateTimeOffset now)
    {
        if (Status == OfferStatus.Accepted)
            return;

        EnsureTurnOf(by, now);

        Raise(new OfferAccepted(by));
    }

    public void Reject(string reason)
    {
        if (Status == OfferStatus.Rejected)
            return;

        if (!IsOpen)
            throw new DomainException("Solo se puede rechazar una oferta abierta.");

        Raise(new OfferRejected(Guard.RequireReason(reason, OfferPolicy.MaxReasonLength)));
    }

    public void Withdraw()
    {
        if (Status == OfferStatus.Withdrawn)
            return;

        if (!IsOpen)
            throw new DomainException("Solo se puede retirar una oferta abierta.");

        Raise(new OfferWithdrawn());
    }

    public void Expire(DateTimeOffset now)
    {
        if (!IsOpen || now < RespondBy)
            return;

        Raise(new OfferExpired());
    }

    public void FallThrough(string reason)
    {
        if (Status == OfferStatus.FellThrough)
            return;

        if (Status != OfferStatus.Accepted)
            throw new DomainException("Solo una oferta aceptada puede caerse.");

        Raise(new OfferFellThrough(Guard.RequireReason(reason, OfferPolicy.MaxReasonLength)));
    }

    public override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case OfferSubmitted e: When(e); break;
            case OfferCountered e: When(e); break;
            case OfferAccepted e: When(e); break;
            case OfferRejected: Status = OfferStatus.Rejected; break;
            case OfferWithdrawn: Status = OfferStatus.Withdrawn; break;
            case OfferExpired: Status = OfferStatus.Expired; break;
            case OfferFellThrough: Status = OfferStatus.FellThrough; break;

            default:
                throw new DomainException(
                    $"El evento '{domainEvent.GetType().Name}' no pertenece al agregado Offer.");
        }
    }

    private void When(OfferSubmitted e)
    {
        Id = e.OfferId;
        ListingId = e.ListingId;
        OffererId = e.OffererId;
        Operation = e.Operation;
        ListedPrice = Money.FromStorage(e.ListedPrice);
        Amount = Money.FromStorage(e.Amount);
        RentTerms = e.RentStartDate is { } start && e.RentTermMonths is { } months
            ? RentTerms.FromStorage(start, months)
            : null;
        Status = OfferStatus.AwaitingPublisher;
        LastMoveBy = OfferParty.Offerer;
        CounterOfferCount = 0;
        RespondBy = e.RespondBy;
    }

    private void When(OfferCountered e)
    {
        Amount = Money.FromStorage(e.Amount);
        LastMoveBy = e.By;
        CounterOfferCount++;
        RespondBy = e.RespondBy;
        Status = e.By == OfferParty.Publisher ? OfferStatus.AwaitingOfferer : OfferStatus.AwaitingPublisher;
    }

    private void When(OfferAccepted e)
    {
        LastMoveBy = e.By;
        Status = OfferStatus.Accepted;
    }

    private void EnsureTurnOf(OfferParty by, DateTimeOffset now)
    {
        if (!Enum.IsDefined(by))
            throw new DomainException("La parte que responde no es válida.");

        if (!IsOpen)
            throw new DomainException("La oferta ya no está abierta.");

        var expected = by == OfferParty.Publisher ? OfferStatus.AwaitingPublisher : OfferStatus.AwaitingOfferer;

        if (Status != expected)
            throw new DomainException("No es el turno de esta parte para responder.");

        if (now >= RespondBy)
            throw new DomainException("El plazo para responder la oferta ya venció.");
    }

    private static void EnsureAboveMinimum(Money amount, Money listedPrice)
    {
        var minimum = (long)Math.Ceiling(listedPrice.Amount * OfferPolicy.MinOfferRatio);

        if (amount.Amount < minimum)
            throw new DomainException(
                $"La oferta debe ser al menos el {OfferPolicy.MinOfferRatio * 100:0}% del precio publicado ({Money.Create(minimum)}).");
    }
}
