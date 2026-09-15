namespace NotificationLog.RentalService.Domain.Inquiries;

public interface IInquiryRepository
{
    Task<Inquiry?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

    Task AppendAsync(Inquiry inquiry, CancellationToken cancellationToken = default);
}
