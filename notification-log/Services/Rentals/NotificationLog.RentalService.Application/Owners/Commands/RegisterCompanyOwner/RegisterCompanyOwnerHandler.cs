using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using FluentValidation;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Application.Owners.Producers;
using NotificationLog.RentalService.Application.Owners.Queries;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Owners.ValueObjects;

namespace NotificationLog.RentalService.Application.Owners.Commands.RegisterCompanyOwner;

public sealed class RegisterCompanyOwnerHandler
{
    private readonly IOwnerRepository _owners;
    private readonly IOwnerReadModel _readModel;
    private readonly IAccountProvisioner _accounts;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<RegisterCompanyOwnerCommand> _validator;

    public RegisterCompanyOwnerHandler(
        IOwnerRepository owners,
        IOwnerReadModel readModel,
        IAccountProvisioner accounts,
        IUnitOfWork uow,
        IValidator<RegisterCompanyOwnerCommand> validator)
        => (_owners, _readModel, _accounts, _uow, _validator) = (owners, readModel, accounts, uow, validator);

    public async Task<Guid> HandleAsync(RegisterCompanyOwnerCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var ownerId = await OwnerIdResolver.ResolveAsync(_owners, user, ct);

        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var legalName = LegalName.Create(cmd.LegalName);
        var nit = Nit.Create(cmd.Nit);
        var contact = ContactInfo.Create(cmd.Email, cmd.Phone);

        if (await _readModel.ExistsWithNitAsync(nit.Number, ct))
            throw new AppValidationException("Ya existe un propietario con ese NIT.");

        var owner = Owner.RegisterCompany(ownerId, user.GetUserId(), legalName, nit, contact);

        await _owners.AppendAsync(owner, ct);
        await _uow.SaveChangesAsync(ct);


        await _accounts.RequestAccountAsync(new AccountCreationRequested()
        {
            UserId = owner.Id.ToString(),
            Email = contact.Email,
            Phone = contact.Phone,
            Name = legalName.Value,
            Locale = string.Empty,
            TimeZone = string.Empty,
            AcceptsNotifications = true,
            Role = UserRole.Propietario.ToString()
        }, ct);

        return owner.Id;
    }
}
