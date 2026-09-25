using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Listings.Commands.ApproveListing;
using NotificationLog.RentalService.Application.Listings.Commands.ChangeListingPrice;
using NotificationLog.RentalService.Application.Listings.Commands.CloseListing;
using NotificationLog.RentalService.Application.Listings.Commands.DraftListing;
using NotificationLog.RentalService.Application.Listings.Commands.ExpireListing;
using NotificationLog.RentalService.Application.Listings.Commands.PauseListing;
using NotificationLog.RentalService.Application.Listings.Commands.ReinstateListing;
using NotificationLog.RentalService.Application.Listings.Commands.RejectListing;
using NotificationLog.RentalService.Application.Listings.Commands.RenewListing;
using NotificationLog.RentalService.Application.Listings.Commands.ResumeListing;
using NotificationLog.RentalService.Application.Listings.Commands.SubmitListingForReview;
using NotificationLog.RentalService.Application.Listings.Commands.SuspendListing;
using NotificationLog.RentalService.Application.Listings.Commands.UpdateListingDetails;
using NotificationLog.RentalService.Application.Listings.Commands.UpdateListingPhotos;
using NotificationLog.RentalService.Application.Listings.Commands.WarnListingExpiry;
using NotificationLog.RentalService.Application.Listings.Commands.WithdrawListing;
using NotificationLog.RentalService.Application.Owners.Commands.RegisterCompanyOwner;
using NotificationLog.RentalService.Application.Owners.Commands.RegisterNaturalOwner;
using NotificationLog.RentalService.Application.Processes;
using NotificationLog.RentalService.Application.Listings.Queries;
using NotificationLog.RentalService.Application.Owners.Queries;
using NotificationLog.RentalService.Application.Visitors.Commands.CompleteVisitorProfile;
using NotificationLog.RentalService.Application.Visitors.Queries;

namespace NotificationLog.RentalService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly,
            includeInternalTypes: true);

        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<ApproveListingHandler>();
        services.AddScoped<ChangeListingPriceHandler>();
        services.AddScoped<CloseListingHandler>();
        services.AddScoped<DraftListingHandler>();
        services.AddScoped<ExpireListingHandler>();
        services.AddScoped<PauseListingHandler>();
        services.AddScoped<ReinstateListingHandler>();
        services.AddScoped<RejectListingHandler>();
        services.AddScoped<RenewListingHandler>();
        services.AddScoped<ResumeListingHandler>();
        services.AddScoped<SubmitListingForReviewHandler>();
        services.AddScoped<SuspendListingHandler>();
        services.AddScoped<UpdateListingDetailsHandler>();
        services.AddScoped<UpdateListingPhotosHandler>();
        services.AddScoped<WarnListingExpiryHandler>();
        services.AddScoped<WithdrawListingHandler>();

        services.AddScoped<RegisterCompanyOwnerHandler>();
        services.AddScoped<RegisterNaturalOwnerHandler>();

        services.AddScoped<CompleteVisitorProfileHandler>();

        services.AddScoped<ListListingsHandler>();
        services.AddScoped<GetListingByIdHandler>();
        services.AddScoped<GetOwnerByIdHandler>();
        services.AddScoped<ListOwnersHandler>();
        services.AddScoped<GetVisitorByIdHandler>();
        services.AddScoped<ListVisitorsHandler>();

        services.AddScoped<ListingLifecycleProcess>();
        services.AddScoped<ListingNotificationsProcess>();
        services.AddScoped<ProcessRouter>();

        return services;
    }
}
