namespace NotificationLog.IdentityService.Api.Accounts;

public sealed record SignedInUser(
    Guid Id,
    string? Email,
    string? Phone,
    string SecurityStamp,
    IReadOnlyList<string> Roles)
{
    public string DisplayName => Email ?? Phone ?? Id.ToString();
}
