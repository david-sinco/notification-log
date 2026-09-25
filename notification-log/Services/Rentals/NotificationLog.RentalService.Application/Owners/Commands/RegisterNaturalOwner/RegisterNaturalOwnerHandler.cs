using Application.Shared.Abstractions;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using FluentValidation;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Application.Common.Producers;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Owners;
using System.Security.Claims;

namespace NotificationLog.RentalService.Application.Owners.Commands.RegisterNaturalOwner;

public sealed class RegisterNaturalOwnerHandler(
    IOwnerRepository owners,
    IAccountProvisioner accounts,
    IUnitOfWork uow,
    IValidator<RegisterNaturalOwnerCommand> validator)
{
    private readonly IOwnerRepository _owners = owners;
    private readonly IAccountProvisioner _accounts = accounts;
    private readonly IUnitOfWork _uow = uow;
    private readonly IValidator<RegisterNaturalOwnerCommand> _validator = validator;

    public async Task<Guid> HandleAsync(RegisterNaturalOwnerCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var ownerId = await OwnerIdResolver.ResolveAsync(_owners, user, ct);

        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var name = PersonName.Create(cmd.FirstNames, cmd.LastNames);
        var document = IdentityDocument.Create(cmd.DocumentType, cmd.DocumentNumber);
        var contact = ContactInfo.Create(cmd.Email, cmd.Phone);

        if (!await _owners.TryReserveNaturalAsync(ownerId, document, ct))
            throw new AppValidationException("Ya existe un propietario con ese documento.");

        var owner = Owner.RegisterNatural(ownerId, user.GetUserId(), name, document, contact);

        await _owners.AppendAsync(owner, ct);
        await _uow.SaveChangesAsync(ct);

        await _accounts.RequestAccountAsync(new AccountCreationRequested()
        {
            UserId = owner.Id.ToString(),
            Email = contact.Email,
            Phone = contact.Phone,
            Name = name.ToString(),
            Locale = string.Empty,
            TimeZone = string.Empty,
            AcceptsNotifications = true,
            Role = UserRole.Propietario.ToString()
        }, ct);

        return owner.Id;
    }
}
