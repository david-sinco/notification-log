using Application.Shared.Abstractions;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using FluentValidation;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Application.Common.Producers;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Visitors;
using System.Security.Claims;

namespace NotificationLog.RentalService.Application.Visitors.Commands.RegisterVisitor;

public sealed class RegisterVisitorHandler
{
    private readonly IVisitorRepository _visitorsRepo;
    private readonly IAccountProvisioner _accounts;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<RegisterVisitorCommand> _validator;

    public RegisterVisitorHandler(
        IVisitorRepository visitorsRepo,
        IAccountProvisioner accounts,
        IUnitOfWork uow,
        IValidator<RegisterVisitorCommand> validator)
        => (_visitorsRepo, _accounts, _uow, _validator) = (visitorsRepo, accounts, uow, validator);

    public async Task<Guid> HandleAsync(RegisterVisitorCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var visitorId = Guid.NewGuid();

        var name = PersonName.Create(cmd.FirstNames, cmd.LastNames);
        var document = IdentityDocument.Create(cmd.DocumentType, cmd.DocumentNumber);
        var contact = ContactInfo.Create(cmd.Email, cmd.Phone);

        if (!await _visitorsRepo.TryReserveVisitorAsync(visitorId, document, ct))
            throw new AppValidationException("Ya existe un visitante con ese documento.");

        var visitor = Visitor.RegisterVisitor(visitorId, name, document, contact);


        await _visitorsRepo.AppendAsync(visitor, ct);
        await _uow.SaveChangesAsync(ct);

        await _accounts.RequestAccountAsync(new AccountCreationRequested()
        {
            UserId = visitorId.ToString(),
            Email = contact.Email,
            Phone = contact.Phone,
            Name = name.ToString(),
            Locale = string.Empty,
            TimeZone = string.Empty,
            AcceptsNotifications = true,
            Role = UserRole.Visitor.ToString()
        }, ct);

        return visitorId;
    }
}
