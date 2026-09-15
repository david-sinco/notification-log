namespace NotificationLog.RentalService.Api.Contracts.Offers;

public sealed record SubmitOfferRequest(Guid OffererId, Guid ListingId, long Amount, DateOnly? RentStartDate, int? RentTermMonths);
