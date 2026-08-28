using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.EventLog;
using Users.Domain;
using Users.Infrastructure.EventLog;

namespace Users.Infrastructure.Materializer;

/// <summary>
/// Owns the only copy of Users' state in this architecture. On boot it replays users.events from
/// the true beginning; after catch-up it keeps consuming live; command handling (StartAsync /
/// AppendAsync, called from InMemoryUserRepository) also happens here, because producing an event
/// and deciding whether it's allowed to happen at all (email uniqueness) share the same lock this
/// type owns. Nothing here is durable except the log — restart the process and it rebuilds from
/// zero, which is the point (event-driven/README.md).
/// </summary>
public sealed class UserMaterializer(IEventLog eventLog, ILogger<UserMaterializer> logger) : BackgroundService
{
    // One process-wide lock, not one per user id. Email uniqueness is the one cross-aggregate
    // invariant in this domain, and unlike event-sourcing/'s Postgres-backed reservation row,
    // there is no database left to enforce it if two DIFFERENT users' commands race against each
    // other — a per-user lock would reopen exactly that TOCTOU gap. The cost, stated plainly
    // rather than hidden: every command is serialized behind one Kafka round-trip, because the
    // produce await happens *inside* this lock, not released around it — that's what keeps
    // "produced in order per partition => safe to advance a monotonic watermark" true, including
    // against the live consumer loop below, which takes the same lock for the same reason.
    private readonly SemaphoreSlim _commandLock = new(1, 1);

    // Guards the dictionaries themselves against a concurrent unprotected reader (TryGet /
    // TryGetHistory, called from HTTP GET handlers that have no reason to queue behind
    // _commandLock's Kafka round-trips). Every mutation below happens while already holding
    // _commandLock, so this is only ever contended by a reader, never by two writers.
    private readonly object _gate = new();

    private readonly Dictionary<Guid, User> _users = [];
    private readonly Dictionary<string, Guid> _emailOwners = [];
    private readonly Dictionary<Guid, List<object>> _history = [];

    // The direct analogue of event-sourcing/SPEC.md §8's "consumer drops snapshots at or below
    // the stored version" — there's no version field on this wire, so the offset a partition has
    // been applied through IS the version check. A just-produced record can become visible to the
    // live consumer loop before the producing command's own await returns, so whichever of the two
    // paths below observes a given offset first must win; "offset <= watermark" is correct
    // regardless of which one that is.
    private readonly Dictionary<int, long> _appliedThroughOffset = [];

    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public bool IsReady => _ready.Task.IsCompletedSuccessfully;

    public Task WaitUntilReadyAsync(CancellationToken ct) => _ready.Task.WaitAsync(ct);

    public User? TryGet(Guid userId)
    {
        lock (_gate)
        {
            var stored = _users.GetValueOrDefault(userId);
            return stored is null ? null : Clone(stored);
        }
    }

    public IReadOnlyList<object>? TryGetHistory(Guid userId)
    {
        lock (_gate)
            return _history.TryGetValue(userId, out var list) ? list.ToArray() : null;
    }

    /// <summary>
    /// Callers (UserCommandService, via IUserRepository.LoadAsync) mutate the User they get back
    /// via User.Apply *before* handing it to StartAsync/AppendAsync — that's how command handling
    /// stays symmetric with a plain replay (see this type's own class comment). If TryGet returned
    /// the same reference stored in _users, that mutation would land directly on the canonical
    /// state, and AppendAsync's "did VerifiedEmail change?" comparison against "previous" would be
    /// comparing the object against itself — always equal, silently disabling the email-uniqueness
    /// check entirely. Every property here is a primitive or an immutable record that's always
    /// *replaced*, never mutated in place, in User.Apply, so a shallow copy is a complete copy.
    /// </summary>
    private static User Clone(User source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        VerifiedEmail = source.VerifiedEmail,
        VerifiedPhone = source.VerifiedPhone,
        PendingEmail = source.PendingEmail,
        PendingPhone = source.PendingPhone,
        Preferences = source.Preferences,
        IsActive = source.IsActive,
        Version = source.Version,
    };

    public async Task StartAsync(User user, UserRegistered registered, CancellationToken ct)
    {
        await _commandLock.WaitAsync(ct);
        try
        {
            var (partition, offset) = await eventLog.ProduceAsync(
                UsersEventCodec.Topic, user.Id.ToString(), UsersEventCodec.Encode(user.Id, registered), ct);

            lock (_gate)
            {
                _users[user.Id] = user;
                _history[user.Id] = [registered];
                AdvanceWatermarkLocked(partition, offset);
            }
        }
        finally
        {
            _commandLock.Release();
        }
    }

    public async Task AppendAsync(Guid userId, User user, IReadOnlyList<object> newEvents, CancellationToken ct)
    {
        await _commandLock.WaitAsync(ct);
        try
        {
            User? previous;
            lock (_gate)
                previous = _users.GetValueOrDefault(userId);

            // The check that matters happens BEFORE anything is produced: EmailAlreadyInUseException
            // here must leave the log untouched, the same way event-sourcing/'s reservation-row
            // conflict never reaches SaveChangesAsync.
            if (user.VerifiedEmail is { } newEmail &&
                !string.Equals(newEmail, previous?.VerifiedEmail, StringComparison.Ordinal))
            {
                lock (_gate)
                {
                    if (_emailOwners.TryGetValue(newEmail, out var owner) && owner != userId)
                        throw new EmailAlreadyInUseException(newEmail);
                }
            }

            (int Partition, long Offset) last = default;
            foreach (var e in newEvents)
                last = await eventLog.ProduceAsync(
                    UsersEventCodec.Topic, userId.ToString(), UsersEventCodec.Encode(userId, e), ct);

            lock (_gate)
            {
                if (user.VerifiedEmail is { } confirmedEmail &&
                    !string.Equals(confirmedEmail, previous?.VerifiedEmail, StringComparison.Ordinal))
                {
                    if (previous?.VerifiedEmail is { } oldEmail)
                        _emailOwners.Remove(oldEmail);
                    _emailOwners[confirmedEmail] = userId;
                }

                _users[userId] = user;
                if (!_history.TryGetValue(userId, out var list))
                    _history[userId] = list = [];
                list.AddRange(newEvents);

                if (newEvents.Count > 0)
                    AdvanceWatermarkLocked(last.Partition, last.Offset);
            }
        }
        finally
        {
            _commandLock.Release();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await eventLog.EnsureTopicAsync(
            UsersEventCodec.Topic, UsersEventCodec.Partitions, UsersEventCodec.TopicConfig, stoppingToken);

        var startedAt = DateTimeOffset.UtcNow;
        var replayed = 0;

        await eventLog.SubscribeAsync(
            UsersEventCodec.Topic,
            consumerGroupId: $"users-materializer-{Guid.NewGuid()}",
            onRecord: async (record, ct) =>
            {
                await _commandLock.WaitAsync(ct);
                try
                {
                    ApplyIfNew(record.Partition, record.Offset, Guid.Parse(record.Key), record.Value);
                    replayed++;
                }
                finally
                {
                    _commandLock.Release();
                }
            },
            onCaughtUp: _ =>
            {
                logger.LogInformation(
                    "users.events per-user materializer replayed {Count} events in {Ms}ms, caught up to the current high-watermark.",
                    replayed, (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds);
                _ready.TrySetResult();
                return Task.CompletedTask;
            },
            stoppingToken);
    }

    /// <summary>Called while already holding _commandLock, from the live consumer loop only —
    /// the command path applies directly in StartAsync/AppendAsync instead.</summary>
    private void ApplyIfNew(int partition, long offset, Guid userId, ReadOnlyMemory<byte> rawValue)
    {
        lock (_gate)
        {
            if (_appliedThroughOffset.TryGetValue(partition, out var applied) && offset <= applied)
                return;

            var domainEvent = UsersEventCodec.Decode(rawValue);

            if (!_users.TryGetValue(userId, out var user))
            {
                if (domainEvent is not UserRegistered registered)
                {
                    // Cannot happen with a single producer and Kafka's per-partition ordering —
                    // UserRegistered is always the first record for a given key — but a consumer
                    // must never crash on a wire surprise, so log and drop rather than throw.
                    logger.LogWarning(
                        "Received {Type} for unknown user {UserId} before any UserRegistered; dropping.",
                        domainEvent.GetType().Name, userId);
                    AdvanceWatermarkLocked(partition, offset);
                    return;
                }

                user = User.Create(registered);
                _users[userId] = user;
                _history[userId] = [registered];
            }
            else
            {
                var previousEmail = user.VerifiedEmail;
                user.Apply(domainEvent);

                if (!_history.TryGetValue(userId, out var list))
                    _history[userId] = list = [];
                list.Add(domainEvent);

                if (user.VerifiedEmail is { } newEmail && !string.Equals(newEmail, previousEmail, StringComparison.Ordinal))
                {
                    if (previousEmail is not null)
                        _emailOwners.Remove(previousEmail);
                    _emailOwners[newEmail] = userId;
                }
            }

            AdvanceWatermarkLocked(partition, offset);
        }
    }

    /// <summary>Caller must already hold _gate.</summary>
    private void AdvanceWatermarkLocked(int partition, long offset)
    {
        if (!_appliedThroughOffset.TryGetValue(partition, out var current) || offset > current)
            _appliedThroughOffset[partition] = offset;
    }
}
