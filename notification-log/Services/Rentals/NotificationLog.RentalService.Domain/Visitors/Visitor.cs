using Domain.Shared.Common;
using Domain.Shared.EventSourcing;
using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Shared;
using NotificationLog.RentalService.Domain.Visitors.Events;

namespace NotificationLog.RentalService.Domain.Visitors;

public sealed class Visitor: AggregateRoot
{
    private Visitor() { }

    public PersonName? Name { get; private set; }
    public IdentityDocument? Document { get; private set; }
    public ContactInfo? Contact { get; private set; }

    public static Visitor RegisterVisitor(Guid id, PersonName name, IdentityDocument document, ContactInfo contact)
    {
        Guard.RequireId(id, "El identificador del visitante es obligatorio.");
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(contact);

        var visitor = new Visitor();

        visitor.Raise(new VisitorRegistered(
            id, name.FirstNames, name.LastNames, document.Type, document.Number,
            contact.Email, contact.Phone));

        return visitor;
    }

    public override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case VisitorRegistered e: When(e); break;

            default:
                throw new DomainException(
                    $"El evento '{domainEvent.GetType().Name}' no pertenece al agregado Visitor.");
        }
    }

    private void When(VisitorRegistered e)
    {
        Id = e.VisitorId;
        Name = PersonName.FromStorage(e.FirstNames, e.LastNames);
        Document = IdentityDocument.FromStorage(e.DocumentType, e.DocumentNumber);
        Contact = ContactInfo.FromStorage(e.Email, e.Phone);
    }
}
