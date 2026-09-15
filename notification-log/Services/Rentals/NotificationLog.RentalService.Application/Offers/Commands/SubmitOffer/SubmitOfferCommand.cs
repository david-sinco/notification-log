namespace NotificationLog.RentalService.Application.Offers.Commands.SubmitOffer;

public sealed record SubmitOfferCommand(
    Guid OffererId,
    Guid ListingId,
    long Amount,
    DateOnly? RentStartDate,
    int? RentTermMonths);
