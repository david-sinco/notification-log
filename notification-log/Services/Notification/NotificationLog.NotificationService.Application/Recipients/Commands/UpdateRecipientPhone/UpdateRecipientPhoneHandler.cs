using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientPhone;

public sealed class UpdateRecipientPhoneHandler
{
    private readonly IRecipientRepository _recipients;
    private readonly IUnitOfWork _uow;

    public UpdateRecipientPhoneHandler(IRecipientRepository recipients, IUnitOfWork uow)
        => (_recipients, _uow) = (recipients, uow);

    public async Task HandleAsync(UpdateRecipientPhoneCommand cmd, CancellationToken ct)
    {
        var recipient = await _recipients.GetByIdAsync(cmd.RecipientId, ct)
            ?? throw new NotFoundException(nameof(Recipient), cmd.RecipientId);

        recipient.ChangePhone(cmd.Phone);

        await _uow.SaveChangesAsync(ct);
    }
}
