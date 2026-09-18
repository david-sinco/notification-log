namespace NotificationLog.Web.Api.Rentals;

public enum Operation
{
    Sale = 0,
    Rent = 1
}

public enum ListingStatus
{
    Draft = 0,
    InReview = 1,
    Published = 2,
    Paused = 3,
    Expired = 4,
    Closed = 5,
    Withdrawn = 6,
    Suspended = 7
}

public enum PropertyType
{
    Apartment = 0,
    Studio = 1
}

public enum RejectionReason
{
    LowQualityPhotos = 0,
    InconsistentData = 1,
    SuspiciousPrice = 2,
    InvalidAddress = 3,
    ProhibitedContent = 4
}

public enum VisitStatus
{
    Requested = 0,
    Confirmed = 1,
    Declined = 2,
    Expired = 3,
    Cancelled = 4,
    Completed = 5,
    NoShow = 6
}
