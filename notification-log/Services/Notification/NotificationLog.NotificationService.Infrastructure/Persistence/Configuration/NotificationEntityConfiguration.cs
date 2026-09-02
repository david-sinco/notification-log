using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationLog.NotificationService.Domain.Notifications;

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Configuration;

// Se llama "Entity" y no "NotificationConfiguration" (el patrón <Tipo>Configuration de las demás
// clases de esta carpeta) porque ese nombre ya lo usa la configuración EF de
// Domain.Triggers.NotificationConfiguration, un agregado hijo completamente distinto.
internal sealed class NotificationEntityConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();

        builder.Property(n => n.EventKey)
            .HasMaxLength(Notification.EventKeyMaxLength)
            .IsRequired();

        builder.Property(n => n.ConfigurationId).IsRequired();
        builder.Property(n => n.TemplateId).IsRequired();
        builder.Property(n => n.TemplateVersionId).IsRequired();
        builder.Property(n => n.RecipientId).IsRequired();

        builder.Property(n => n.Channel).HasConversion<int>().IsRequired();
        builder.Property(n => n.Status).HasConversion<int>().IsRequired();

        builder.Property(n => n.Destination)
            .HasMaxLength(Notification.DestinationMaxLength)
            .IsRequired();

        builder.Property(n => n.ProviderMessageId)
            .HasMaxLength(Notification.ProviderMessageIdMaxLength);

        builder.Property(n => n.Error)
            .HasMaxLength(Notification.ErrorMaxLength);

        builder.Property(n => n.OccurredAt).IsRequired();

        builder.HasIndex(n => n.RecipientId)
            .HasDatabaseName("IX_Notifications_RecipientId");

        builder.HasIndex(n => n.EventKey)
            .HasDatabaseName("IX_Notifications_EventKey");

        builder.HasIndex(n => n.Status)
            .HasDatabaseName("IX_Notifications_Status");

        // Payload: datos libres del evento de negocio en el momento del envío, no una
        // relación de entidades, así que se mapea el campo privado como JSON (mismo
        // enfoque que Recipient.Attributes en RecipientConfiguration).
        builder.Ignore(n => n.Payload);

        var payloadComparer = new ValueComparer<Dictionary<string, string>>(
            (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
            d => d.Aggregate(0, (hash, kv) => HashCode.Combine(hash, kv.Key, kv.Value)),
            d => new Dictionary<string, string>(d));

        builder.Property<Dictionary<string, string>>("_payload")
            .HasField("_payload")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("Payload")
            .HasConversion(
                dict => JsonSerializer.Serialize(dict, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? new Dictionary<string, string>()
                    : JsonSerializer.Deserialize<Dictionary<string, string>>(json, (JsonSerializerOptions?)null)!,
                payloadComparer)
            .IsRequired();
    }
}
