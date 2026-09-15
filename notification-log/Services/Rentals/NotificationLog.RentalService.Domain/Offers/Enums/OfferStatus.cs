namespace NotificationLog.RentalService.Domain.Offers.Enums;

public enum OfferStatus
{
    AwaitingPublisher,
    AwaitingOfferer,
    Accepted,
    Rejected,
    Withdrawn,
    Expired,
    FellThrough
}
