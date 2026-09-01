using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Application.Recipients.Dtos;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Queries.GetRecipientById;

public sealed class GetRecipientByIdHandler
{
    private readonly IRecipientRepository _recipients;

    public GetRecipientByIdHandler(IRecipientRepository recipients)
        => _recipients = recipients;

    public async Task<RecipientDto> HandleAsync(GetRecipientByIdQuery query, CancellationToken ct)
    {
        var recipient = await _recipients.GetByIdAsync(query.Id, ct)
            ?? throw new NotFoundException(nameof(Recipient), query.Id);

        return new RecipientDto(
            recipient.Id,
            recipient.Name,
            recipient.Email,
            recipient.Phone,
            recipient.Locale,
            recipient.TimeZone,
            recipient.Attributes,
            recipient.IsActive);
    }
}