namespace NotificationLog.IdentityService.Application.Users.Queries.ValidateSession;

public sealed record ValidateSessionQuery(Guid UserId, string SecurityStamp);
