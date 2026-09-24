using Domain.Shared.Common;
using Domain.Shared.EventSourcing;
using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Shared;
using NotificationLog.RentalService.Domain.Visitors.Enums;
using NotificationLog.RentalService.Domain.Visitors.Events;

namespace NotificationLog.RentalService.Domain.Visitors;

public sealed class Visitor : AggregateRoot
{
    private Visitor() { }

    public VisitorStatus Status { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public PersonName? Name { get; private set; }
    public IdentityDocument? Document { get; private set; }
    public ContactInfo? Contact { get; private set; }

    public static Visitor Register(Guid id, string name, string email, string phone)
    {
        Guard.RequireId(id, "El identificador del visitante es obligatorio.");

        var visitor = new Visitor();

        visitor.Raise(new VisitorRegistered(id, name.Trim(), email.Trim(), phone.Trim()));

        return visitor;
    }

    public void CompleteProfile(PersonName name, IdentityDocument document, ContactInfo contact)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(contact);

        if (Status == VisitorStatus.Registered)
            throw new DomainException("El visitante ya completó su registro.");

        Raise(new VisitorProfileCompleted(
            Id, name.FirstNames, name.LastNames, document.Type, document.Number,
            contact.Email, contact.Phone));
    }

    public override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case VisitorRegistered e: When(e); break;
            case VisitorProfileCompleted e: When(e); break;

            default:
                throw new DomainException(
                    $"El evento '{domainEvent.GetType().Name}' no pertenece al agregado Visitor.");
        }
    }

    private void When(VisitorRegistered e)
    {
        Id = e.VisitorId;
        Status = VisitorStatus.PendingProfile;
        DisplayName = e.Name;
    }

    private void When(VisitorProfileCompleted e)
    {
        Status = VisitorStatus.Registered;
        Name = PersonName.FromStorage(e.FirstNames, e.LastNames);
        DisplayName = Name.ToString();
        Document = IdentityDocument.FromStorage(e.DocumentType, e.DocumentNumber);
        Contact = ContactInfo.FromStorage(e.Email, e.Phone);
    }
}
