using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using FluentValidation;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Visitors;

namespace NotificationLog.RentalService.Application.Visitors.Commands.CompleteVisitorProfile;

public sealed class CompleteVisitorProfileHandler(
    IVisitorRepository visitors,
    IUnitOfWork uow,
    IValidator<CompleteVisitorProfileCommand> validator)
{
    private readonly IVisitorRepository _visitors = visitors;
    private readonly IUnitOfWork _uow = uow;
    private readonly IValidator<CompleteVisitorProfileCommand> _validator = validator;

    public async Task HandleAsync(CompleteVisitorProfileCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var visitorId = user.GetUserId();

        var name = PersonName.Create(cmd.FirstNames, cmd.LastNames);
        var document = IdentityDocument.Create(cmd.DocumentType, cmd.DocumentNumber);
        var contact = ContactInfo.Create(cmd.Email, cmd.Phone);

        var visitor = await _visitors.LoadAsync(visitorId, ct)
            ?? Visitor.Register(visitorId, name.ToString(), contact.Email, contact.Phone);

        visitor.CompleteProfile(name, document, contact);

        if (!await _visitors.TryReserveVisitorAsync(visitorId, document, ct))
            throw new AppValidationException("Ya existe un visitante con ese documento.");

        await _visitors.AppendAsync(visitor, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
