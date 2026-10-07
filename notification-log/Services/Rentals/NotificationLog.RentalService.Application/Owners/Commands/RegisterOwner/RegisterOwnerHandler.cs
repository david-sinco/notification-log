using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using FluentValidation;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Owners.ValueObjects;

namespace NotificationLog.RentalService.Application.Owners.Commands.RegisterOwner;

public sealed class RegisterOwnerHandler(
    IOwnerRepository owners,
    IUnitOfWork uow,
    IValidator<RegisterOwnerCommand> validator)
{
    private readonly IOwnerRepository _owners = owners;
    private readonly IUnitOfWork _uow = uow;
    private readonly IValidator<RegisterOwnerCommand> _validator = validator;

    public async Task<Guid> HandleAsync(RegisterOwnerCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var ownerId = Guid.NewGuid();
        var relatedUserId = await RelatedUserResolver.ResolveAsync(_owners, ownerId, user, ct);

        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var name = OwnerName.Create(cmd.Name);
        var contact = ContactInfo.Create(cmd.Email, cmd.Phone);

        if (!await _owners.TryReserveEmailAsync(ownerId, contact.Email, ct))
            throw new AppValidationException("Ya existe un propietario con ese correo.");

        if (!await _owners.TryReservePhoneAsync(ownerId, contact.Phone, ct))
            throw new AppValidationException("Ya existe un propietario con ese teléfono.");

        var owner = Owner.Register(ownerId, user.GetUserId(), relatedUserId, name, contact);

        await _owners.AppendAsync(owner, ct);
        await _uow.SaveChangesAsync(ct);

        return owner.Id;
    }
}
