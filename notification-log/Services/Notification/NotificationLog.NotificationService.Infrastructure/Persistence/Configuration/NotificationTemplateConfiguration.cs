using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationLog.NotificationService.Domain.Templates;

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Configuration;

internal sealed class NotificationTemplateConfiguration
    : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Name)
            .HasConversion(
                name => name.Value,
                value => TemplateName.Create(value))
            .HasColumnName("Name")
            .HasMaxLength(TemplateName.MaxLength)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(t => t.Name)
            .IsUnique()
            .HasDatabaseName("UX_NotificationTemplates_Name");

        builder.Property(t => t.Channel)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.Subject)
            .HasMaxLength(NotificationTemplate.SubjectMaxLength);

        builder.Property(t => t.Body)
            .HasMaxLength(NotificationTemplate.BodyMaxLength)
            .IsRequired();

        builder.Property(t => t.IsEnabled).IsRequired();

        builder.HasIndex(t => new { t.Channel, t.IsEnabled })
            .HasDatabaseName("IX_NotificationTemplates_Channel_IsEnabled");
    }
}