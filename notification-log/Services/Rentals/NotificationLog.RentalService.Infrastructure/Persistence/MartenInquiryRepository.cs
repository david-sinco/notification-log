using NotificationLog.RentalService.Domain.Inquiries;

namespace NotificationLog.RentalService.Infrastructure.Persistence;

internal sealed class MartenInquiryRepository : IInquiryRepository
{
    private readonly AggregateStreams _streams;

    public MartenInquiryRepository(AggregateStreams streams) => _streams = streams;

    public Task<Inquiry?> LoadAsync(Guid id, CancellationToken cancellationToken = default)
        => _streams.LoadAsync<Inquiry>(id, cancellationToken);

    public Task AppendAsync(Inquiry inquiry, CancellationToken cancellationToken = default)
    {
        _streams.Append(inquiry);
        return Task.CompletedTask;
    }
}
