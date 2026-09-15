using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Inquiries;
using NotificationLog.RentalService.Domain.Inquiries.Events;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Events;
using NotificationLog.RentalService.Domain.Offers;
using NotificationLog.RentalService.Domain.Offers.Enums;
using NotificationLog.RentalService.Domain.Offers.Events;
using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;
using NotificationLog.RentalService.Domain.Visits.Events;

namespace NotificationLog.RentalService.Application.Processes;

public sealed class ListingNotificationsProcess
{
    private readonly IListingRepository _listings;
    private readonly IOfferRepository _offers;
    private readonly IVisitRepository _visits;
    private readonly IInquiryRepository _inquiries;
    private readonly INotificationDispatcher _notifications;

    public ListingNotificationsProcess(
        IListingRepository listings,
        IOfferRepository offers,
        IVisitRepository visits,
        IInquiryRepository inquiries,
        INotificationDispatcher notifications)
        => (_listings, _offers, _visits, _inquiries, _notifications) = (listings, offers, visits, inquiries, notifications);

    public Task NotifyAsync(Guid streamId, IDomainEvent domainEvent, CancellationToken ct) => domainEvent switch
    {
        ListingApproved => ToHostAsync(streamId, NotificationKeys.ListingApproved, ct),
        ListingRejected e => ToHostAsync(streamId, NotificationKeys.ListingRejected, ct, ("reasons", string.Join(",", e.Reasons))),
        ListingExpired => ToHostAsync(streamId, NotificationKeys.ListingExpired, ct),
        ListingSuspended e => ToHostAsync(streamId, NotificationKeys.ListingSuspended, ct, ("reason", e.Reason)),
        ListingClosed => ToHostAsync(streamId, NotificationKeys.ListingClosed, ct),
        ReservationReleased e => ToHostAsync(streamId, NotificationKeys.ReservationReleased, ct, ("reason", e.Reason)),
        InquiryOpened e => ToHostAsync(e.ListingId, NotificationKeys.InquiryNew, ct, ("inquiry_id", e.InquiryId.ToString())),
        InquiryMessagePosted e => OnInquiryMessageAsync(streamId, e, ct),
        VisitRequested e => ToHostAsync(e.ListingId, NotificationKeys.VisitRequested, ct, ("visit_id", e.VisitId.ToString())),
        VisitConfirmed or VisitDeclined or VisitCancelled => OnVisitChangedAsync(streamId, domainEvent, ct),
        OfferSubmitted e => ToHostAsync(e.ListingId, NotificationKeys.OfferReceived, ct, ("offer_id", e.OfferId.ToString())),
        OfferCountered or OfferAccepted or OfferRejected or OfferExpired => OnOfferChangedAsync(streamId, domainEvent, ct),
        _ => Task.CompletedTask
    };

    private async Task ToHostAsync(Guid listingId, string key, CancellationToken ct, params (string Key, string Value)[] extra)
    {
        if (await _listings.LoadAsync(listingId, ct) is not { } listing)
            return;

        var data = extra.ToDictionary(item => item.Key, item => item.Value);
        data["listing_id"] = listingId.ToString();

        await _notifications.DispatchAsync(key, ListingAccess.HostOf(listing), data, ct);
    }

    private async Task OnInquiryMessageAsync(Guid inquiryId, InquiryMessagePosted e, CancellationToken ct)
    {
        if (await _inquiries.LoadAsync(inquiryId, ct) is not { } inquiry
            || await _listings.LoadAsync(inquiry.ListingId, ct) is not { } listing)
            return;

        var fromSeeker = e.AuthorId == inquiry.SeekerId;

        await _notifications.DispatchAsync(
            fromSeeker ? NotificationKeys.InquiryNew : NotificationKeys.InquiryReply,
            fromSeeker ? ListingAccess.HostOf(listing) : inquiry.SeekerId,
            new Dictionary<string, string>
            {
                ["inquiry_id"] = inquiryId.ToString(),
                ["listing_id"] = listing.Id.ToString()
            },
            ct);
    }

    private async Task OnVisitChangedAsync(Guid visitId, IDomainEvent domainEvent, CancellationToken ct)
    {
        if (await _visits.LoadAsync(visitId, ct) is not { } visit)
            return;

        var (key, recipient) = domainEvent switch
        {
            VisitConfirmed => (NotificationKeys.VisitConfirmed, visit.VisitorId),
            VisitDeclined => (NotificationKeys.VisitDeclined, visit.VisitorId),
            VisitCancelled c => (NotificationKeys.VisitCancelled, c.By == VisitParty.Visitor ? visit.HostId : visit.VisitorId),
            _ => throw new ArgumentOutOfRangeException(nameof(domainEvent))
        };

        await _notifications.DispatchAsync(
            key,
            recipient,
            new Dictionary<string, string>
            {
                ["visit_id"] = visitId.ToString(),
                ["listing_id"] = visit.ListingId.ToString()
            },
            ct);
    }

    private async Task OnOfferChangedAsync(Guid offerId, IDomainEvent domainEvent, CancellationToken ct)
    {
        if (await _offers.LoadAsync(offerId, ct) is not { } offer
            || await _listings.LoadAsync(offer.ListingId, ct) is not { } listing)
            return;

        var host = ListingAccess.HostOf(listing);

        var (key, recipient) = domainEvent switch
        {
            OfferCountered c => (NotificationKeys.OfferCountered, c.By == OfferParty.Publisher ? offer.OffererId : host),
            OfferAccepted a => (NotificationKeys.OfferAccepted, a.By == OfferParty.Offerer ? host : offer.OffererId),
            OfferRejected => (NotificationKeys.OfferRejected, offer.OffererId),
            OfferExpired => (NotificationKeys.OfferExpired, offer.OffererId),
            _ => throw new ArgumentOutOfRangeException(nameof(domainEvent))
        };

        await _notifications.DispatchAsync(
            key,
            recipient,
            new Dictionary<string, string>
            {
                ["offer_id"] = offerId.ToString(),
                ["listing_id"] = listing.Id.ToString(),
                ["amount"] = offer.Amount.Amount.ToString()
            },
            ct);
    }
}
