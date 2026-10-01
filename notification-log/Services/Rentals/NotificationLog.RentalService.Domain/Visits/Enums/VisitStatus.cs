namespace NotificationLog.RentalService.Domain.Visits.Enums;

public enum VisitStatus
{
    AwaitingHost,
    AwaitingVisitor,
    Scheduled,
    Completed,
    NoShow,
    Cancelled,
    Expired
}
