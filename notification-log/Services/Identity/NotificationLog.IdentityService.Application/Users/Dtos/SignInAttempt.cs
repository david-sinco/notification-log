namespace NotificationLog.IdentityService.Application.Users.Dtos;

public sealed record SignInAttempt(SignInStatus Status, SignedInUser? User)
{
    public static SignInAttempt Failed(SignInStatus status) => new(status, null);
}
