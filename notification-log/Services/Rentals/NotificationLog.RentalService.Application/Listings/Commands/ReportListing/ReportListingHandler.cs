using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.ReportListing;

public sealed class ReportListingHandler
{
    private readonly IListingRepository _listings;
    private readonly ListingAccess _access;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<ReportListingCommand> _validator;

    public ReportListingHandler(
        IListingRepository listings, ListingAccess access, IUnitOfWork uow, IValidator<ReportListingCommand> validator)
        => (_listings, _access, _uow, _validator) = (listings, access, uow, validator);

    public async Task HandleAsync(ReportListingCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);
        await _access.RequireVerifiedUserAsync(cmd.ReporterId, requireDocument: false, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        listing.Report(cmd.ReporterId, cmd.Reason);

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
