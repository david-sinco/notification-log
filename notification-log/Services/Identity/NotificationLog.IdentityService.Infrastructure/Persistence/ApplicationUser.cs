using Microsoft.AspNetCore.Identity;

namespace NotificationLog.IdentityService.Infrastructure.Persistence;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser() { }

    public ApplicationUser(Guid id)
    {
        Id = id;
        UserName = id.ToString();
    }

    public string? Name { get; set; }

    public string? Locale { get; set; }

    public string? TimeZone { get; set; }

    public bool AcceptsNotifications { get; set; }
}
