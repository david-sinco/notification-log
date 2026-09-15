using Domain.Shared.Common;
using Domain.Shared.EventSourcing;
using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Inquiries.Events;
using NotificationLog.RentalService.Domain.Inquiries.ValueObjects;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Domain.Inquiries;

public sealed class Inquiry : AggregateRoot
{
    public const int MaxReasonLength = 500;
    public const int MaxNewInquiriesPerDay = 10;

    private Inquiry() { }

    public Guid ListingId { get; private set; }
    public Guid SeekerId { get; private set; }
    public bool IsClosed { get; private set; }

    public static Inquiry Open(Guid listingId, Guid seekerId, InquiryMessage message)
    {
        Guard.RequireId(listingId, "La publicación de la consulta es obligatoria.");
        Guard.RequireId(seekerId, "El interesado es obligatorio.");
        ArgumentNullException.ThrowIfNull(message);

        var inquiry = new Inquiry();

        inquiry.Raise(new InquiryOpened(InquiryId.For(listingId, seekerId), listingId, seekerId, message.Text));

        return inquiry;
    }

    public void Post(Guid authorId, InquiryMessage message)
    {
        Guard.RequireId(authorId, "El autor del mensaje es obligatorio.");
        ArgumentNullException.ThrowIfNull(message);

        if (IsClosed)
            throw new DomainException("La consulta está cerrada.");

        Raise(new InquiryMessagePosted(authorId, message.Text));
    }

    public void Close(string reason)
    {
        if (IsClosed)
            return;

        Raise(new InquiryClosed(Guard.RequireReason(reason, MaxReasonLength)));
    }

    public override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case InquiryOpened e: When(e); break;
            case InquiryMessagePosted: break;
            case InquiryClosed: IsClosed = true; break;

            default:
                throw new DomainException(
                    $"El evento '{domainEvent.GetType().Name}' no pertenece al agregado Inquiry.");
        }
    }

    private void When(InquiryOpened e)
    {
        Id = e.InquiryId;
        ListingId = e.ListingId;
        SeekerId = e.SeekerId;
        IsClosed = false;
    }
}
