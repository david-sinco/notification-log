using Domain.Shared.Common;
using Domain.Shared.EventSourcing;
using Domain.Shared.Exceptions;
using NotificationLog.UserService.Domain.Users.Events;

namespace NotificationLog.UserService.Domain.Users;

public sealed class User : AggregateRoot
{
    private User() { }

    public PersonName Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Phone? Phone { get; private set; }
    public bool IsEmailConfirmed { get; private set; }
    public bool IsPhoneConfirmed { get; private set; }
    public bool IsActive { get; private set; }

    public static User Register(
        Guid id,
        PersonName name,
        Email email,
        Phone? phone)
    {
        if (id == Guid.Empty)
            throw new DomainException("El identificador del usuario es obligatorio.");

        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(email);

        var user = new User();

        user.Raise(new UserRegistered(
            id,
            name.Value,
            email.Value,
            phone?.Value ?? string.Empty));

        return user;
    }

    public void ChangeName(PersonName name)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(name);

        if (name == Name)
            return;

        Raise(new UserNameChanged(name.Value));
    }

    public void ChangeEmail(Email email)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(email);

        if (email == Email)
            return;

        Raise(new EmailChanged(email.Value));
    }

    public void ConfirmEmail(Email confirmed)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(confirmed);

        if (confirmed != Email)
            throw new DomainException(
                "El correo verificado ya no es el correo actual del usuario; solicita una nueva verificación.");

        if (IsEmailConfirmed)
            return;

        Raise(new EmailConfirmed(Email.Value));
    }

    public void ChangePhone(Phone phone)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(phone);

        if (phone == Phone)
            return;

        Raise(new PhoneChanged(phone.Value));
    }

    public void ConfirmPhone(Phone confirmed)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(confirmed);

        if (Phone is null)
            throw new DomainException("El usuario no tiene un teléfono que verificar.");

        if (confirmed != Phone)
            throw new DomainException(
                "El teléfono verificado ya no es el teléfono actual del usuario; solicita una nueva verificación.");

        if (IsPhoneConfirmed)
            return;

        Raise(new PhoneConfirmed(Phone.Value));
    }

    public void Deactivate(string? reason = null)
    {
        if (!IsActive)
            return;

        Raise(new UserDeactivated(string.IsNullOrWhiteSpace(reason) ? null : reason.Trim()));
    }

    public void Reactivate()
    {
        if (IsActive)
            return;

        Raise(new UserReactivated());
    }


    public override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case UserRegistered e: When(e); break;
            case UserNameChanged e: When(e); break;
            case EmailChanged e: When(e); break;
            case EmailConfirmed e: When(e); break;
            case PhoneChanged e: When(e); break;
            case PhoneConfirmed e: When(e); break;
            case UserDeactivated: IsActive = false; break;
            case UserReactivated: IsActive = true; break;

            default:
                throw new DomainException(
                    $"El evento '{domainEvent.GetType().Name}' no pertenece al agregado User.");
        }
    }

    private void When(UserRegistered e)
    {
        Id = e.UserId;
        Name = PersonName.FromStorage(e.Name);
        Email = Email.FromStorage(e.Email);
        Phone = string.IsNullOrEmpty(e.Phone) ? null : Phone.FromStorage(e.Phone);
        IsEmailConfirmed = false;
        IsPhoneConfirmed = false;
        IsActive = true;
    }

    private void When(UserNameChanged e) => Name = PersonName.FromStorage(e.Name);

    private void When(EmailChanged e)
    {
        Email = Email.FromStorage(e.Email);
        IsEmailConfirmed = false;
    }

    private void When(EmailConfirmed e)
    {
        Email = Email.FromStorage(e.Email);
        IsEmailConfirmed = true;
    }

    private void When(PhoneChanged e)
    {
        Phone = Phone.FromStorage(e.Phone);
        IsPhoneConfirmed = false;
    }

    private void When(PhoneConfirmed e)
    {
        Phone = Phone.FromStorage(e.Phone);
        IsPhoneConfirmed = true;
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new DomainException("El usuario está desactivado.");
    }
}
