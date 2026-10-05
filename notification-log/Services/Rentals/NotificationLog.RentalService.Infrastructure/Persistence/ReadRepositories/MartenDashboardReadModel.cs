using Marten;
using NotificationLog.RentalService.Application.Common.Storage;
using NotificationLog.RentalService.Application.Dashboard.Queries;
using NotificationLog.RentalService.Application.Dashboard.Queries.Dtos;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Visits.Enums;
using NotificationLog.RentalService.Infrastructure.Persistence.Views;

namespace NotificationLog.RentalService.Infrastructure.Persistence.ReadRepositories;

internal sealed class MartenDashboardReadModel : IDashboardReadModel
{
    private const int RecentCount = 5;

    private readonly IQuerySession _session;
    private readonly IPhotoUrlProvider _photoUrls;

    public MartenDashboardReadModel(IQuerySession session, IPhotoUrlProvider photoUrls)
    {
        _session = session;
        _photoUrls = photoUrls;
    }

    public async Task<DashboardDto> GetAsync(Guid userId, bool isStaff, CancellationToken ct)
    {
        var batch = _session.CreateBatchQuery();

        var inReview = batch.Query<ListingView>().Where(x => x.Status == ListingStatus.InReview).Count();
        var oldestInReview = batch.Query<ListingView>()
            .Where(x => x.Status == ListingStatus.InReview)
            .OrderBy(x => x.UpdatedAt)
            .Take(1)
            .ToList();
        var published = batch.Query<ListingView>().Where(x => x.Status == ListingStatus.Published).Count();
        var myDrafts = batch.Query<ListingView>()
            .Where(x => x.Status == ListingStatus.Draft && (x.OwnerId == userId || x.CreatedBy == userId))
            .Count();
        var recent = batch.Query<ListingView>().OrderByDescending(x => x.UpdatedAt).Take(RecentCount).ToList();

        var awaitingCount = batch.Query<VisitView>()
            .Where(x => x.Status == VisitStatus.AwaitingHost && (isStaff || x.HostId == userId))
            .Count();
        var nextVisit = batch.Query<VisitView>()
            .Where(x => x.Status == VisitStatus.AwaitingHost && (isStaff || x.HostId == userId))
            .OrderBy(x => x.RespondBy)
            .Take(1)
            .ToList();

        var owners = isStaff ? batch.Query<OwnerView>().Count() : null;
        var closed = isStaff ? null : batch.Query<ListingView>().Where(x => x.Status == ListingStatus.Closed).Count();

        await batch.Execute(ct);

        var recentListings = await recent;
        var ownerNames = (await _session.LoadManyAsync<OwnerView>(ct, recentListings.Select(x => x.OwnerId).Distinct().ToArray()))
            .ToDictionary(x => x.Id, x => x.DisplayName());

        return new DashboardDto(
            (int)await inReview,
            (await oldestInReview).FirstOrDefault()?.UpdatedAt,
            (int)await awaitingCount,
            (await nextVisit).FirstOrDefault()?.RespondBy,
            (int)await myDrafts,
            (int)await published,
            owners is null ? null : (int)await owners,
            closed is null ? null : (int)await closed,
            recentListings
                .Select(x => MartenListingReadModel.ToSummary(x, ownerNames.GetValueOrDefault(x.OwnerId), _photoUrls))
                .ToList());
    }
}
