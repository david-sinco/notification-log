using Domain.Shared.Common;
using Domain.Shared.EventSourcing;
using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Common;
using NotificationLog.RentalService.Domain.Visitors.Events;
using NotificationLog.RentalService.Domain.Visitors.Exceptions;

namespace NotificationLog.RentalService.Domain.Visitors;

public sealed class Visitor : AggregateRoot
{
    private Visitor() { }

    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;

    public static Visitor Register(Guid id, string name, string email, string phone)
    {
        Guard.RequireId(id, "El identificador del visitante es obligatorio.");

        var visitor = new Visitor();

        visitor.Raise(new VisitorRegistered(id, name.Trim(), email.Trim(), phone.Trim()));

        return visitor;
    }

    public void Update(string name, string email, string phone)
    {
        var newName = string.IsNullOrWhiteSpace(name) ? Name : name.Trim();
        var newEmail = string.IsNullOrWhiteSpace(email) ? Email : email.Trim();
        var newPhone = string.IsNullOrWhiteSpace(phone) ? Phone : phone.Trim();

        if (newName == Name && newEmail == Email && newPhone == Phone)
            throw new VisitorUnchangedException(Id);

        Raise(new VisitorUpdated(Id, newName, newEmail, newPhone));
    }

    public override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case VisitorRegistered e: When(e); break;
            case VisitorUpdated e: When(e); break;

            default:
                throw new DomainException(
                    $"El evento '{domainEvent.GetType().Name}' no pertenece al agregado Visitor.");
        }
    }

    private void When(VisitorRegistered e)
    {
        Id = e.VisitorId;
        Name = e.Name;
        Email = e.Email;
        Phone = e.Phone;
    }

    private void When(VisitorUpdated e)
    {
        Name = e.Name;
        Email = e.Email;
        Phone = e.Phone;
    }
}
