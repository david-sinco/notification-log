using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotificationLog.IdentityService.Infrastructure.Persistence;

public sealed class IdentityServiceDbContextFactory : IDesignTimeDbContextFactory<IdentityServiceDbContext>
{
    public IdentityServiceDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdentityServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=identity;Username=postgres;Password=postgres")
            .Options;

        return new IdentityServiceDbContext(options);
    }
}
