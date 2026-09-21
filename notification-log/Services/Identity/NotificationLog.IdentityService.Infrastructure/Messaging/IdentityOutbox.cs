using NotificationLog.IdentityService.Infrastructure.Persistence;
using Wolverine.EntityFrameworkCore;

namespace NotificationLog.IdentityService.Infrastructure.Messaging;

internal sealed class IdentityOutbox
{
    private readonly IDbContextOutbox<IdentityServiceDbContext> _outbox;

    public IdentityOutbox(IDbContextOutbox<IdentityServiceDbContext> outbox) => _outbox = outbox;

    public ValueTask PublishAsync<T>(T message) => _outbox.PublishAsync(message);

    public Task SaveChangesAndFlushAsync(CancellationToken ct) => _outbox.SaveChangesAndFlushMessagesAsync(ct);
}
