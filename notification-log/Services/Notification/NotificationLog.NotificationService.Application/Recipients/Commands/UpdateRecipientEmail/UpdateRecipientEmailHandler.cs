using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientEmail;

public sealed class UpdateRecipientEmailHandler
{
    private readonly IRecipientRepository _recipients;
    private readonly IUnitOfWork _uow;

    public UpdateRecipientEmailHandler(IRecipientRepository recipients, IUnitOfWork uow)
        => (_recipients, _uow) = (recipients, uow);

    public async Task HandleAsync(UpdateRecipientEmailCommand cmd, CancellationToken ct)
    {
        var recipient = await _recipients.GetByIdAsync(cmd.RecipientId, ct)
            ?? throw new NotFoundException(nameof(Recipient), cmd.RecipientId);

        recipient.ChangeEmail(cmd.Email);

        await _uow.SaveChangesAsync(ct);
    }
}