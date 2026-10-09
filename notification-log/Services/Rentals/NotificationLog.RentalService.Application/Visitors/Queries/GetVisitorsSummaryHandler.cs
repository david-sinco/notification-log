using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;

namespace NotificationLog.RentalService.Application.Visitors.Queries;

public sealed class GetVisitorsSummaryHandler(IVisitorReadModel visitors, TimeProvider time)
{
    private static readonly TimeSpan NewVisitorWindow = TimeSpan.FromDays(7);

    private readonly IVisitorReadModel _visitors = visitors;
    private readonly TimeProvider _time = time;

    public Task<VisitorsSummaryDto> HandleAsync(CancellationToken ct)
        => _visitors.GetSummaryAsync(_time.GetUtcNow() - NewVisitorWindow, ct);
}
