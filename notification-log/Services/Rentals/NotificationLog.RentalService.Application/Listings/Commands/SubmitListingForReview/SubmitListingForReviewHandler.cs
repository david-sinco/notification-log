using Application.Shared.Abstractions;
using Application.Shared.Common;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.SubmitListingForReview;

public sealed class SubmitListingForReviewHandler
{
    private readonly IListingRepository _listings;
    private readonly ListingAccess _access;
    private readonly IIdentityReplica _identity;
    private readonly ISoftRuleChecks _rules;
    private readonly IUnitOfWork _uow;

    public SubmitListingForReviewHandler(
        IListingRepository listings, ListingAccess access, IIdentityReplica identity, ISoftRuleChecks rules, IUnitOfWork uow)
        => (_listings, _access, _identity, _rules, _uow) = (listings, access, identity, rules, uow);

    public async Task HandleAsync(SubmitListingForReviewCommand cmd, CancellationToken ct)
    {
        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        await _access.EnsureCanManageAsync(listing, cmd.ActorId, ct);

        var owner = await _identity.GetPersonAsync(listing.PublisherId, ct)
            ?? throw new AppValidationException("El propietario no está registrado en la plataforma.");

        if (!owner.IsDocumentVerified || (listing.AdvisorId is null && !owner.IsPhoneVerified))
            throw new AppValidationException("El propietario necesita tener verificados el teléfono y el documento.");

        if (listing.AdvisorId is { } advisorId)
        {
            var advisor = await _identity.GetAdvisorAsync(advisorId, ct);

            if (advisor is not { IsActive: true })
                throw new AppValidationException("El asesor no está activo.");

            if (listing.Location is { } location
                && !advisor.ServiceCities.Contains(location.City, StringComparer.OrdinalIgnoreCase))
                throw new AppValidationException($"El asesor no presta servicio en {location.City}.");

            if (await _rules.CountActiveListingsByAdvisorAsync(advisorId, ct) >= advisor.Capacity)
                throw new AppValidationException("El asesor ya gestiona el máximo de publicaciones de su capacidad.");
        }
        else if (await _rules.CountActiveListingsAsync(listing.PublisherId, ct) >= ListingPolicy.MaxActiveListingsPerOwner)
        {
            throw new AppValidationException(
                $"Un particular puede tener como máximo {ListingPolicy.MaxActiveListingsPerOwner} publicaciones activas.");
        }

        listing.SubmitForReview();

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
