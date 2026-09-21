namespace NotificationLog.IdentityService.Application.Users.Common;

public sealed record AccountResult(string? Error)
{
    public bool Succeeded => Error is null;

    public static AccountResult Ok() => new((string?)null);

    public static AccountResult Fail(string error) => new(error);
}
