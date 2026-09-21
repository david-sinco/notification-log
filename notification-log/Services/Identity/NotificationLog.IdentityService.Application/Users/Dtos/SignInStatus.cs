namespace NotificationLog.IdentityService.Application.Users.Dtos;

public enum SignInStatus
{
    Succeeded,
    InvalidCredentials,
    NotVerified,
    LockedOut
}
