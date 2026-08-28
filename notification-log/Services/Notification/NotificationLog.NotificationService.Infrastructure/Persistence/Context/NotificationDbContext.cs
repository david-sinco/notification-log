using Microsoft.EntityFrameworkCore;
using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Context;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<NotificationTrigger> Triggers => Set<NotificationTrigger>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(NotificationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}