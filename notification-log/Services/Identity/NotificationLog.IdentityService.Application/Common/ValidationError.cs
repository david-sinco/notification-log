using Application.Shared.Common;

namespace NotificationLog.IdentityService.Application.Common;

public static class ValidationError
{
    public static AppValidationException For(string field, string message) =>
        new(new Dictionary<string, string[]> { [field] = [message] });
}
