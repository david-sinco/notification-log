namespace NotificationLog.IdentityService.Application.Users.Queries.SearchUsers;

public sealed record SearchUsersQuery(string? Search, int Skip = 0, int Take = 20);
