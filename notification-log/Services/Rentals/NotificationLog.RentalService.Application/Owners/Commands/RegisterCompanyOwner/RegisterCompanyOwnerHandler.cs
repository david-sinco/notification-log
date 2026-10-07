using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using FluentValidation;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Owners.ValueObjects;

namespace NotificationLog.RentalService.Application.Owners.Commands.RegisterCompanyOwner;

public sealed class RegisterCompanyOwnerHandler(
    IOwnerRepository owners,
    IUnitOfWork uow,
    IValidator<RegisterCompanyOwnerCommand> validator)
{
    private readonly IOwnerRepository _owners = owners;
    private readonly IUnitOfWork _uow = uow;
    private readonly IValidator<RegisterCompanyOwnerCommand> _validator = validator;

    public async Task<Guid> HandleAsync(RegisterCompanyOwnerCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var ownerId = Guid.NewGuid();
        var relatedUserId = await RelatedUserResolver.ResolveAsync(_owners, ownerId, user, ct);

        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var legalName = LegalName.Create(cmd.LegalName);
        var nit = Nit.Create(cmd.Nit);
        var contact = ContactInfo.Create(cmd.Email, cmd.Phone);

        if (!await _owners.TryReserveCompanyAsync(ownerId, nit, ct))
            throw new AppValidationException("Ya existe un propietario con ese NIT.");

        var owner = Owner.RegisterCompany(ownerId, user.GetUserId(), relatedUserId, legalName, nit, contact);

        await _owners.AppendAsync(owner, ct);
        await _uow.SaveChangesAsync(ct);

        return owner.Id;
    }
}
