using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientAttributes;

public sealed class UpdateRecipientAttributesHandler
{
    private readonly IRecipientRepository _recipients;
    private readonly IUnitOfWork _uow;

    public UpdateRecipientAttributesHandler(IRecipientRepository recipients, IUnitOfWork uow)
        => (_recipients, _uow) = (recipients, uow);

    public async Task HandleAsync(UpdateRecipientAttributesCommand cmd, CancellationToken ct)
    {
        var recipient = await _recipients.GetByIdAsync(cmd.RecipientId, ct)
            ?? throw new NotFoundException(nameof(Recipient), cmd.RecipientId);

        recipient.SetAttributes(cmd.Attributes);

        await _uow.SaveChangesAsync(ct);
    }
}
