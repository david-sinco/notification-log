namespace NotificationLog.NotificationService.Application.Abstractions;

public enum UserChangeKind
{
    Created = 1,
    ProfileUpdated = 2,
    EmailUpdated = 3,
    PhoneUpdated = 4,
    AttributesUpdated = 5,
    StatusUpdated = 6
}