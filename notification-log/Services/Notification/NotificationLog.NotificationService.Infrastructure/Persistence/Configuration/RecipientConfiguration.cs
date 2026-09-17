using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Configuration;

internal sealed class RecipientConfiguration : IEntityTypeConfiguration<Recipient>
{
    public void Configure(EntityTypeBuilder<Recipient> builder)
    {
        builder.ToTable("Recipients");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.Name)
            .HasMaxLength(Recipient.NameMaxLength)
            .IsRequired();

        builder.Property(r => r.Email)
            .HasMaxLength(Recipient.EmailMaxLength);

        builder.HasIndex(r => r.Email)
            .HasDatabaseName("IX_Recipients_Email");

        builder.Property(r => r.Phone)
            .HasMaxLength(Recipient.PhoneMaxLength);

        builder.Property(r => r.Locale)
            .HasMaxLength(Recipient.LocaleMaxLength)
            .IsRequired();

        builder.Property(r => r.TimeZone)
            .HasMaxLength(Recipient.TimeZoneMaxLength)
            .IsRequired();

        builder.Property(r => r.IsActive).IsRequired();

        builder.Property(r => r.AcceptsNotifications).IsRequired();

        builder.HasIndex(r => r.IsActive)
            .HasDatabaseName("IX_Recipients_IsActive");

        // Atributos: diccionario libre, no es una relación de entidades sino un valor
        // serializado del agregado, así que se ignora la propiedad de solo lectura y
        // se mapea directamente el campo privado como JSON.
        builder.Ignore(r => r.Attributes);

        var attributesComparer = new ValueComparer<Dictionary<string, string>>(
            (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
            d => d.Aggregate(0, (hash, kv) => HashCode.Combine(hash, kv.Key, kv.Value)),
            d => new Dictionary<string, string>(d));

        builder.Property<Dictionary<string, string>>("_attributes")
            .HasField("_attributes")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("Attributes")
            .HasConversion(
                dict => JsonSerializer.Serialize(dict, (JsonSerializerOptions?)null),
                json => string.IsNullOrEmpty(json)
                    ? new Dictionary<string, string>()
                    : JsonSerializer.Deserialize<Dictionary<string, string>>(json, (JsonSerializerOptions?)null)!,
                attributesComparer)
            .IsRequired();
    }
}
