namespace NotificationLog.Web.Api.Identity.Users;

public sealed record SetUserRolesRequest(IReadOnlyList<string> Roles);
