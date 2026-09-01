using FluentValidation;
using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientProfile;

public sealed class UpdateRecipientProfileHandler
{
    private readonly IRecipientRepository _recipients;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateRecipientProfileCommand> _validator;

    public UpdateRecipientProfileHandler(
        IRecipientRepository recipients,
        IUnitOfWork uow,
        IValidator<UpdateRecipientProfileCommand> validator)
        => (_recipients, _uow, _validator) = (recipients, uow, validator);

    public async Task HandleAsync(UpdateRecipientProfileCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var recipient = await _recipients.GetByIdAsync(cmd.RecipientId, ct)
            ?? throw new NotFoundException(nameof(Recipient), cmd.RecipientId);

        recipient.ChangeName(cmd.Name);
        recipient.ChangeLocalization(cmd.Locale, cmd.TimeZone);

        await _uow.SaveChangesAsync(ct);
    }
}
