using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Commands.SetRecipientStatus;

public sealed class SetRecipientStatusHandler
{
    private readonly IRecipientRepository _recipients;
    private readonly IUnitOfWork _uow;

    public SetRecipientStatusHandler(IRecipientRepository recipients, IUnitOfWork uow)
        => (_recipients, _uow) = (recipients, uow);

    public async Task HandleAsync(SetRecipientStatusCommand cmd, CancellationToken ct)
    {
        var recipient = await _recipients.GetByIdAsync(cmd.RecipientId, ct)
            ?? throw new NotFoundException(nameof(Recipient), cmd.RecipientId);

        if (cmd.IsActive)
            recipient.Activate();
        else
            recipient.Deactivate();

        await _uow.SaveChangesAsync(ct);
    }
}
