using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Listings.Commands.AddListingPhoto;
using NotificationLog.RentalService.Application.Listings.Commands.ApproveListing;
using NotificationLog.RentalService.Application.Listings.Commands.ChangeListingPrice;
using NotificationLog.RentalService.Application.Listings.Commands.CloseListing;
using NotificationLog.RentalService.Application.Listings.Commands.DraftListing;
using NotificationLog.RentalService.Application.Listings.Commands.ExpireListing;
using NotificationLog.RentalService.Application.Listings.Commands.PauseListing;
using NotificationLog.RentalService.Application.Listings.Commands.ReinstateListing;
using NotificationLog.RentalService.Application.Listings.Commands.RejectListing;
using NotificationLog.RentalService.Application.Listings.Commands.RemoveListingPhoto;
using NotificationLog.RentalService.Application.Listings.Commands.RenewListing;
using NotificationLog.RentalService.Application.Listings.Commands.ReorderListingPhotos;
using NotificationLog.RentalService.Application.Listings.Commands.ResumeListing;
using NotificationLog.RentalService.Application.Listings.Commands.SubmitListingForReview;
using NotificationLog.RentalService.Application.Listings.Commands.SuspendListing;
using NotificationLog.RentalService.Application.Listings.Commands.UpdateListingDetails;
using NotificationLog.RentalService.Application.Listings.Commands.WithdrawListing;
using NotificationLog.RentalService.Application.Owners.Commands.ClaimOwner;
using NotificationLog.RentalService.Application.Owners.Commands.RegisterCompanyOwner;
using NotificationLog.RentalService.Application.Owners.Commands.RegisterNaturalOwner;
using NotificationLog.RentalService.Application.Dashboard.Queries;
using NotificationLog.RentalService.Application.Listings.Queries;
using NotificationLog.RentalService.Application.Owners.Queries;
using NotificationLog.RentalService.Application.Visitors.Commands.CompleteVisitorProfile;
using NotificationLog.RentalService.Application.Visitors.Queries;
using NotificationLog.RentalService.Application.Visits.Commands.CancelVisit;
using NotificationLog.RentalService.Application.Visits.Commands.CounterProposeVisit;
using NotificationLog.RentalService.Application.Visits.Commands.MarkVisitCompleted;
using NotificationLog.RentalService.Application.Visits.Commands.MarkVisitNoShow;
using NotificationLog.RentalService.Application.Visits.Commands.RequestVisit;
using NotificationLog.RentalService.Application.Visits.Commands.ScheduleVisit;
using NotificationLog.RentalService.Application.Visits.Queries;

namespace NotificationLog.RentalService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly,
            includeInternalTypes: true);

        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<AddListingPhotoHandler>();
        services.AddScoped<ApproveListingHandler>();
        services.AddScoped<ChangeListingPriceHandler>();
        services.AddScoped<CloseListingHandler>();
        services.AddScoped<DraftListingHandler>();
        services.AddScoped<ExpireListingHandler>();
        services.AddScoped<PauseListingHandler>();
        services.AddScoped<ReinstateListingHandler>();
        services.AddScoped<RejectListingHandler>();
        services.AddScoped<RemoveListingPhotoHandler>();
        services.AddScoped<RenewListingHandler>();
        services.AddScoped<ReorderListingPhotosHandler>();
        services.AddScoped<ResumeListingHandler>();
        services.AddScoped<SubmitListingForReviewHandler>();
        services.AddScoped<SuspendListingHandler>();
        services.AddScoped<UpdateListingDetailsHandler>();
        services.AddScoped<WithdrawListingHandler>();

        services.AddScoped<RegisterCompanyOwnerHandler>();
        services.AddScoped<RegisterNaturalOwnerHandler>();
        services.AddScoped<ClaimOwnerHandler>();

        services.AddScoped<CompleteVisitorProfileHandler>();

        services.AddScoped<CancelVisitHandler>();
        services.AddScoped<CounterProposeVisitHandler>();
        services.AddScoped<MarkVisitCompletedHandler>();
        services.AddScoped<MarkVisitNoShowHandler>();
        services.AddScoped<RequestVisitHandler>();
        services.AddScoped<ScheduleVisitHandler>();

        services.AddScoped<ListListingsHandler>();
        services.AddScoped<GetListingByIdHandler>();
        services.AddScoped<GetOwnerByIdHandler>();
        services.AddScoped<ListOwnersHandler>();
        services.AddScoped<ListMyOwnersHandler>();
        services.AddScoped<ListClaimableOwnersHandler>();
        services.AddScoped<GetVisitorByIdHandler>();
        services.AddScoped<ListVisitorsHandler>();
        services.AddScoped<GetVisitByIdHandler>();
        services.AddScoped<ListVisitsHandler>();
        services.AddScoped<GetDashboardHandler>();

        return services;
    }
}
