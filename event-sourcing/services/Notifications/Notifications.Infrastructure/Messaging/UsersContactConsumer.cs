using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notifications.Application;
using Notifications.Application.IntegrationEvents;
using Shared.Messaging;

namespace Notifications.Infrastructure.Messaging;

/// <summary>
/// Subscribes to users.contact as group "notifications" (SPEC.md §8). Deserializes into this
/// service's own classes and maps to a local snapshot type; never calls back to Users. Logs a
/// warning — doesn't crash — on a schemaVersion higher than it understands.
/// </summary>
public sealed class UsersContactConsumer(
    IMessageTransport transport, IServiceScopeFactory scopeFactory, ILogger<UsersContactConsumer> logger)
    : BackgroundService
{
    private const int MaxUnderstoodSchemaVersion = 1;

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        transport.SubscribeAsync(IntegrationTopics.UsersContact, "notifications", HandleAsync, stoppingToken);

    private async Task HandleAsync(ReadOnlyMemory<byte> body, CancellationToken ct)
    {
        if (body.IsEmpty)
            return; // tombstone — nothing to replicate

        var envelope = JsonSerializer.Deserialize<Envelope<UserContactUpdated>>(Encoding.UTF8.GetString(body.Span));
        if (envelope is null)
            return;

        if (envelope.SchemaVersion > MaxUnderstoodSchemaVersion)
        {
            logger.LogWarning(
                "Received UserContactUpdated with schemaVersion {SchemaVersion}, higher than the {Understood} this build understands; skipping.",
                envelope.SchemaVersion, MaxUnderstoodSchemaVersion);
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var replication = scope.ServiceProvider.GetRequiredService<ContactReplicationService>();
        await replication.ApplySnapshotAsync(ContactSnapshotMapper.ToSnapshot(envelope), ct);
    }
}
