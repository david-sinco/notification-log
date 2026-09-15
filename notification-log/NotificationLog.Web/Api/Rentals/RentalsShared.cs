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
    Reserved = 5,
    Closed = 6,
    Withdrawn = 7,
    Suspended = 8
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

public enum ReportReason
{
    Fraud = 0,
    NoLongerAvailable = 1,
    FalseData = 2,
    InappropriateContent = 3
}

public enum OfferStatus
{
    AwaitingPublisher = 0,
    AwaitingOfferer = 1,
    Accepted = 2,
    Rejected = 3,
    Withdrawn = 4,
    Expired = 5,
    FellThrough = 6
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

public enum AlertFrequency
{
    Immediate = 0,
    Daily = 1,
    None = 2
}
