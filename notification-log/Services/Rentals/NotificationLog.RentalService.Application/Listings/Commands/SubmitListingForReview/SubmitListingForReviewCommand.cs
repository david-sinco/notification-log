namespace NotificationLog.RentalService.Application.Listings.Commands.SubmitListingForReview;

public sealed record SubmitListingForReviewCommand(Guid ActorId, Guid ListingId);
