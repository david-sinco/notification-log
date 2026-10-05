using FluentValidation;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipient;

public sealed class UpdateRecipientHandler
{
    private readonly IRecipientRepository _recipients;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateRecipientCommand> _validator;

    public UpdateRecipientHandler(
        IRecipientRepository recipients, IUnitOfWork uow, IValidator<UpdateRecipientCommand> validator)
        => (_recipients, _uow, _validator) = (recipients, uow, validator);

    public async Task HandleAsync(UpdateRecipientCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var recipient = await _recipients.GetByIdAsync(cmd.RecipientId, ct)
            ?? throw new NotFoundException(nameof(Recipient), cmd.RecipientId);

        if (cmd.Name is not null)
            recipient.ChangeName(cmd.Name);

        recipient.ChangeEmail(cmd.IsEmailVerified ? cmd.Email : null);
        recipient.ChangePhone(cmd.IsPhoneVerified ? cmd.Phone : null);
        recipient.ChangeLocalization(cmd.Locale, cmd.TimeZone);

        if (cmd.AcceptsNotifications is { } acceptsNotifications)
            recipient.ChangeNotificationConsent(acceptsNotifications);

        await _uow.SaveChangesAsync(ct);
    }
}
