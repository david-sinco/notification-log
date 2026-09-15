using Application.Shared.Abstractions;
using NotificationLog.RentalService.Domain.Inquiries;

namespace NotificationLog.RentalService.Application.Inquiries.Commands.CloseInquiry;

public sealed class CloseInquiryHandler
{
    private readonly IInquiryRepository _inquiries;
    private readonly IUnitOfWork _uow;

    public CloseInquiryHandler(IInquiryRepository inquiries, IUnitOfWork uow)
        => (_inquiries, _uow) = (inquiries, uow);

    public async Task HandleAsync(CloseInquiryCommand cmd, CancellationToken ct)
    {
        if (await _inquiries.LoadAsync(cmd.InquiryId, ct) is not { } inquiry)
            return;

        inquiry.Close(cmd.Reason);

        await _inquiries.AppendAsync(inquiry, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
