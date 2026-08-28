using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Configuration;

internal sealed class NotificationTriggerConfiguration
    : IEntityTypeConfiguration<NotificationTrigger>
{
    public void Configure(EntityTypeBuilder<NotificationTrigger> builder)
    {
        builder.ToTable("NotificationTriggers");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.EventKey)
            .HasConversion(
                key => key.Value,
                value => EventKey.Create(value))
            .HasColumnName("EventKey")
            .HasMaxLength(EventKey.MaxLength)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(t => t.EventKey)
            .IsUnique()
            .HasDatabaseName("UX_NotificationTriggers_EventKey");

        builder.Property(t => t.Description)
            .HasMaxLength(NotificationTrigger.DescriptionMaxLength)
            .IsRequired();

        builder.Property(t => t.IsEnabled).IsRequired();

        builder.HasMany(t => t.Configurations)
            .WithOne()
            .HasForeignKey("NotificationTriggerId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(NotificationTrigger.Configurations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}