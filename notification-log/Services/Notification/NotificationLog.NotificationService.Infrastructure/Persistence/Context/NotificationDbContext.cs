using Microsoft.EntityFrameworkCore;
using Application.Shared.Abstractions;
using NotificationLog.NotificationService.Domain.Notifications;
using NotificationLog.NotificationService.Domain.Recipients;
using NotificationLog.NotificationService.Domain.Templates;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Context;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<NotificationTrigger> Triggers => Set<NotificationTrigger>();
    public DbSet<NotificationTemplate> Templates => Set<NotificationTemplate>();
    public DbSet<Recipient> Recipients => Set<Recipient>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(NotificationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}