using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationLog.NotificationService.Domain.Templates;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Configuration;

internal sealed class NotificationConfigurationConfiguration
    : IEntityTypeConfiguration<NotificationConfiguration>
{
    public void Configure(EntityTypeBuilder<NotificationConfiguration> builder)
    {
        builder.ToTable("NotificationConfigurations");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.TemplateId).IsRequired();
        builder.Property(c => c.Channel).HasConversion<int>().IsRequired();
        builder.Property(c => c.IsEnabled).IsRequired();

        builder.HasIndex("NotificationTriggerId", nameof(NotificationConfiguration.Channel))
            .HasDatabaseName("IX_NotificationConfigurations_Trigger_Channel");

        builder.HasOne<NotificationTemplate>()
            .WithMany()
            .HasForeignKey(c => c.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}