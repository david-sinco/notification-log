using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Favorites.Commands.AddFavorite;
using NotificationLog.RentalService.Application.Favorites.Commands.RemoveFavorite;
using NotificationLog.RentalService.Application.Inquiries.Commands.CloseInquiry;
using NotificationLog.RentalService.Application.Inquiries.Commands.ReplyToInquiry;
using NotificationLog.RentalService.Application.Inquiries.Commands.SendInquiryMessage;
using NotificationLog.RentalService.Application.Listings.Commands.AssignAdvisor;
using NotificationLog.RentalService.Application.Listings.Commands.CancelReservation;
using NotificationLog.RentalService.Application.Listings.Commands.ChangeListingPrice;
using NotificationLog.RentalService.Application.Listings.Commands.CloseListing;
using NotificationLog.RentalService.Application.Listings.Commands.DraftListing;
using NotificationLog.RentalService.Application.Listings.Commands.ExpireListing;
using NotificationLog.RentalService.Application.Listings.Commands.ExpireReservation;
using NotificationLog.RentalService.Application.Listings.Commands.ExtendReservation;
using NotificationLog.RentalService.Application.Listings.Commands.ReinstateListing;
using NotificationLog.RentalService.Application.Listings.Commands.RenewListing;
using NotificationLog.RentalService.Application.Listings.Commands.ReportListing;
using NotificationLog.RentalService.Application.Listings.Commands.ReviewListing;
using NotificationLog.RentalService.Application.Listings.Commands.SetListingAvailability;
using NotificationLog.RentalService.Application.Listings.Commands.SubmitListingForReview;
using NotificationLog.RentalService.Application.Listings.Commands.SuspendListing;
using NotificationLog.RentalService.Application.Listings.Commands.UpdateListingDetails;
using NotificationLog.RentalService.Application.Listings.Commands.UpdateListingPhotos;
using NotificationLog.RentalService.Application.Listings.Commands.WarnListingExpiry;
using NotificationLog.RentalService.Application.Listings.Commands.WithdrawListing;
using NotificationLog.RentalService.Application.Offers.Commands.AcceptOffer;
using NotificationLog.RentalService.Application.Offers.Commands.CounterOffer;
using NotificationLog.RentalService.Application.Offers.Commands.ExpireOffer;
using NotificationLog.RentalService.Application.Offers.Commands.RejectOffer;
using NotificationLog.RentalService.Application.Offers.Commands.SubmitOffer;
using NotificationLog.RentalService.Application.Offers.Commands.WithdrawOffer;
using NotificationLog.RentalService.Application.SavedSearches.Commands.CreateSavedSearch;
using NotificationLog.RentalService.Application.SavedSearches.Commands.DeleteSavedSearch;
using NotificationLog.RentalService.Application.SavedSearches.Commands.UpdateSavedSearch;
using NotificationLog.RentalService.Application.Visits.Commands.AutoCompleteVisit;
using NotificationLog.RentalService.Application.Visits.Commands.CancelVisit;
using NotificationLog.RentalService.Application.Visits.Commands.ConfirmVisit;
using NotificationLog.RentalService.Application.Visits.Commands.DeclineVisit;
using NotificationLog.RentalService.Application.Visits.Commands.ExpireVisitRequest;
using NotificationLog.RentalService.Application.Visits.Commands.ReportVisitOutcome;
using NotificationLog.RentalService.Application.Visits.Commands.RequestVisit;
using NotificationLog.RentalService.Application.Visits.Commands.SendVisitReminder;
using NotificationLog.RentalService.Application.Processes;
using NotificationLog.RentalService.Application.Favorites.Queries.GetFavorites;
using NotificationLog.RentalService.Application.Inquiries.Queries.GetInquiryById;
using NotificationLog.RentalService.Application.Inquiries.Queries.ListInquiries;
using NotificationLog.RentalService.Application.Listings.Queries.GetListingById;
using NotificationLog.RentalService.Application.Listings.Queries.ListListings;
using NotificationLog.RentalService.Application.Offers.Queries.GetOfferById;
using NotificationLog.RentalService.Application.Offers.Queries.ListOffers;
using NotificationLog.RentalService.Application.SavedSearches.Queries.GetSavedSearches;
using NotificationLog.RentalService.Application.Visits.Queries.GetVisitById;
using NotificationLog.RentalService.Application.Visits.Queries.ListVisits;

namespace NotificationLog.RentalService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly,
            includeInternalTypes: true);

        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<ListingAccess>();
        services.AddScoped<OfferAccess>();

        services.AddScoped<AddFavoriteHandler>();
        services.AddScoped<RemoveFavoriteHandler>();

        services.AddScoped<CloseInquiryHandler>();
        services.AddScoped<ReplyToInquiryHandler>();
        services.AddScoped<SendInquiryMessageHandler>();

        services.AddScoped<AssignAdvisorHandler>();
        services.AddScoped<CancelReservationHandler>();
        services.AddScoped<ChangeListingPriceHandler>();
        services.AddScoped<CloseListingHandler>();
        services.AddScoped<DraftListingHandler>();
        services.AddScoped<ExpireListingHandler>();
        services.AddScoped<ExpireReservationHandler>();
        services.AddScoped<ExtendReservationHandler>();
        services.AddScoped<ReinstateListingHandler>();
        services.AddScoped<RenewListingHandler>();
        services.AddScoped<ReportListingHandler>();
        services.AddScoped<ReviewListingHandler>();
        services.AddScoped<SetListingAvailabilityHandler>();
        services.AddScoped<SubmitListingForReviewHandler>();
        services.AddScoped<SuspendListingHandler>();
        services.AddScoped<UpdateListingDetailsHandler>();
        services.AddScoped<UpdateListingPhotosHandler>();
        services.AddScoped<WarnListingExpiryHandler>();
        services.AddScoped<WithdrawListingHandler>();

        services.AddScoped<AcceptOfferHandler>();
        services.AddScoped<CounterOfferHandler>();
        services.AddScoped<ExpireOfferHandler>();
        services.AddScoped<RejectOfferHandler>();
        services.AddScoped<SubmitOfferHandler>();
        services.AddScoped<WithdrawOfferHandler>();

        services.AddScoped<CreateSavedSearchHandler>();
        services.AddScoped<DeleteSavedSearchHandler>();
        services.AddScoped<UpdateSavedSearchHandler>();

        services.AddScoped<AutoCompleteVisitHandler>();
        services.AddScoped<CancelVisitHandler>();
        services.AddScoped<ConfirmVisitHandler>();
        services.AddScoped<DeclineVisitHandler>();
        services.AddScoped<ExpireVisitRequestHandler>();
        services.AddScoped<ReportVisitOutcomeHandler>();
        services.AddScoped<RequestVisitHandler>();
        services.AddScoped<SendVisitReminderHandler>();

        services.AddScoped<ListListingsHandler>();
        services.AddScoped<GetListingByIdHandler>();
        services.AddScoped<ListOffersHandler>();
        services.AddScoped<GetOfferByIdHandler>();
        services.AddScoped<ListVisitsHandler>();
        services.AddScoped<GetVisitByIdHandler>();
        services.AddScoped<ListInquiriesHandler>();
        services.AddScoped<GetInquiryByIdHandler>();
        services.AddScoped<GetFavoritesHandler>();
        services.AddScoped<GetSavedSearchesHandler>();

        services.AddScoped<ListingLifecycleProcess>();
        services.AddScoped<OfferProcess>();
        services.AddScoped<VisitProcess>();
        services.AddScoped<SavedSearchMatchingProcess>();
        services.AddScoped<PriceDropProcess>();
        services.AddScoped<ListingNotificationsProcess>();
        services.AddScoped<ProcessRouter>();

        return services;
    }
}
