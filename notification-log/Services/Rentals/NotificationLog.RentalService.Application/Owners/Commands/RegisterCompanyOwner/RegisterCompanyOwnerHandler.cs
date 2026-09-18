using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using FluentValidation;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Owners.ValueObjects;

namespace NotificationLog.RentalService.Application.Owners.Commands.RegisterCompanyOwner;

public sealed class RegisterCompanyOwnerHandler
{
    private readonly IOwnerRepository _owners;
    private readonly IOwnerReadModel _readModel;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<RegisterCompanyOwnerCommand> _validator;

    public RegisterCompanyOwnerHandler(
        IOwnerRepository owners, IOwnerReadModel readModel, IUnitOfWork uow, IValidator<RegisterCompanyOwnerCommand> validator)
        => (_owners, _readModel, _uow, _validator) = (owners, readModel, uow, validator);

    public async Task<Guid> HandleAsync(RegisterCompanyOwnerCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var ownerId = await OwnerIdResolver.ResolveAsync(_owners, user, ct);

        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var legalName = LegalName.Create(cmd.LegalName);
        var nit = Nit.Create(cmd.Nit);

        if (await _readModel.ExistsWithNitAsync(nit.Number, ct))
            throw new AppValidationException("Ya existe un propietario con ese NIT.");

        var owner = Owner.RegisterCompany(ownerId, user.GetUserId(), legalName, nit);

        await _owners.AppendAsync(owner, ct);
        await _uow.SaveChangesAsync(ct);

        return owner.Id;
    }
}
