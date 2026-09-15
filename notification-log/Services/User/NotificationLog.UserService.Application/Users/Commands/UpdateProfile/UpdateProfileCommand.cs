namespace NotificationLog.UserService.Application.Users.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(Guid UserId, string Name, string Email, string? Phone);
