using Microsoft.AspNetCore.Identity;
using NotificationLog.IdentityService.Api.Accounts;

namespace NotificationLog.IdentityService.Api.Data;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser() { }

    public ApplicationUser(Guid id)
    {
        Id = id;
        UserName = id.ToString();
    }

    public bool IsVerified => EmailConfirmed || PhoneNumberConfirmed;

    public bool IsConfirmed(LoginChannel channel) =>
        channel == LoginChannel.Email ? EmailConfirmed : PhoneNumberConfirmed;
}
