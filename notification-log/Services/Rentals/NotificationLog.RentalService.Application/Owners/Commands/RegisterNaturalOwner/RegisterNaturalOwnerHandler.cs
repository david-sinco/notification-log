using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using FluentValidation;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Owners.ValueObjects;

namespace NotificationLog.RentalService.Application.Owners.Commands.RegisterNaturalOwner;

public sealed class RegisterNaturalOwnerHandler
{
    private readonly IOwnerRepository _owners;
    private readonly IOwnerReadModel _readModel;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<RegisterNaturalOwnerCommand> _validator;

    public RegisterNaturalOwnerHandler(
        IOwnerRepository owners, IOwnerReadModel readModel, IUnitOfWork uow, IValidator<RegisterNaturalOwnerCommand> validator)
        => (_owners, _readModel, _uow, _validator) = (owners, readModel, uow, validator);

    public async Task<Guid> HandleAsync(RegisterNaturalOwnerCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var ownerId = await OwnerIdResolver.ResolveAsync(_owners, user, ct);

        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var name = PersonName.Create(cmd.FirstNames, cmd.LastNames);
        var document = IdentityDocument.Create(cmd.DocumentType, cmd.DocumentNumber);

        if (await _readModel.ExistsWithDocumentAsync(document.Type, document.Number, ct))
            throw new AppValidationException("Ya existe un propietario con ese documento.");

        var owner = Owner.RegisterNatural(ownerId, user.GetUserId(), name, document);

        await _owners.AppendAsync(owner, ct);
        await _uow.SaveChangesAsync(ct);

        return owner.Id;
    }
}
