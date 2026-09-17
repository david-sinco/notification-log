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

        // Vacío no significa "no tocar": significa que el dato ya no está verificado.
        recipient.ChangeEmail(cmd.Email);
        recipient.ChangePhone(cmd.Phone);

        await _uow.SaveChangesAsync(ct);
    }
}
