using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationLog.NotificationService.Domain.Templates;

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Configuration;

internal sealed class TemplateVersionConfiguration : IEntityTypeConfiguration<TemplateVersion>
{
    public void Configure(EntityTypeBuilder<TemplateVersion> builder)
    {
        builder.ToTable("TemplateVersions");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.Number).IsRequired();

        builder.Property(v => v.Subject)
            .HasMaxLength(TemplateVersion.SubjectMaxLength);

        builder.Property(v => v.Body)
            .HasMaxLength(TemplateVersion.BodyMaxLength)
            .IsRequired();

        builder.Property(v => v.IsCurrent).IsRequired();

        builder.Property(v => v.CreatedAt).IsRequired();

        builder.HasIndex("NotificationTemplateId", nameof(TemplateVersion.Number))
            .IsUnique()
            .HasDatabaseName("UX_TemplateVersions_Template_Number");
    }
}
