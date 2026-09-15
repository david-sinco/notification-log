using Application.Shared.Common;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Inquiries.Dtos;
using NotificationLog.RentalService.Domain.Inquiries;

namespace NotificationLog.RentalService.Application.Inquiries.Queries.GetInquiryById;

public sealed class GetInquiryByIdHandler
{
    private readonly IInquiryReadModel _inquiries;

    public GetInquiryByIdHandler(IInquiryReadModel inquiries) => _inquiries = inquiries;

    public async Task<InquiryDto> HandleAsync(GetInquiryByIdQuery query, CancellationToken ct)
        => await _inquiries.GetAsync(query.Id, ct) ?? throw new NotFoundException(nameof(Inquiry), query.Id);
}
