using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

        builder.UseOpenIddict();
        builder.MapWolverineEnvelopeStorage(MessagingExtensions.WolverineSchema);
    }
}
