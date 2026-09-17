using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotificationLog.IdentityService.Api.Accounts;
using NotificationLog.IdentityService.Api.Messaging;
using Wolverine.EntityFrameworkCore;

namespace NotificationLog.IdentityService.Api.Data;

public sealed class IdentityServiceDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public IdentityServiceDbContext(DbContextOptions<IdentityServiceDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var user = builder.Entity<ApplicationUser>();

        user.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex").IsUnique();
        user.HasIndex(u => u.PhoneNumber).HasDatabaseName("PhoneNumberIndex").IsUnique();
        user.Property(u => u.Name).HasMaxLength(AccountPolicy.NameMaxLength);
        user.Property(u => u.Locale).HasMaxLength(AccountPolicy.LocaleMaxLength);
        user.Property(u => u.TimeZone).HasMaxLength(AccountPolicy.TimeZoneMaxLength);

        builder.UseOpenIddict();
        builder.MapWolverineEnvelopeStorage(MessagingExtensions.WolverineSchema);
    }
}
