# SPEC — Event Sourcing Lab

A build specification. It is written to be handed to a coding agent with no prior context, so it
includes the reasoning behind each decision, not just the requirements. Where a rule exists to
prevent a specific failure, the failure is named.

---

## 1. Purpose

Build two .NET services that together make it possible to **measure** three things:

1. Marten-driven projections vs. broker-driven projections
2. RabbitMQ vs. Kafka for cross-service state replication
3. Event sourcing vs. plain persistence, side by side in one solution

This is a lab, not a product. When a choice is between "realistic" and "instructive", pick
instructive — but never pick "wrong". The code must be correct enough that the measurements mean
something.

### The core insight being demonstrated

Event-driven architecture and event sourcing are **different axes**, not alternatives:

- **Event sourcing** describes how *one service stores its own state* (internal)
- **EDA** describes how *services communicate* (external)

The two services are deliberately asymmetric to make this visible:

| | Users | Notifications |
|---|---|---|
| Owns its state? | yes, with invariants | no, it replicates | 
| Persistence | event sourced | plain documents |
| Events are | the source of truth | notifications |
| Consistency | strong per aggregate | eventual |

Replicated read state wants last-write-wins keyed by version. Locally owned state with rules
wants an event stream. Do not apply event sourcing to both.

---

## 2. Stack and versions

- **.NET 10** (`net10.0` everywhere)
- **Aspire 13.5.0** for orchestration
- **Marten 8.x** on **PostgreSQL** for both event store and document store
- **RabbitMQ.Client 7.x** and **Confluent.Kafka 2.x**, both implemented, switchable by config
- Central package management via `Directory.Packages.props`

### Aspire 13 project shape — non-negotiable

Aspire went from 9.x straight to 13.x and changed the AppHost project format. Use the 13.x form:

```xml
<Project Sdk="Aspire.AppHost.Sdk/13.5.0">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Aspire.Hosting.PostgreSQL" />
    <PackageReference Include="Aspire.Hosting.RabbitMQ" />
    <PackageReference Include="Aspire.Hosting.Kafka" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Users\Users.Api\Users.Api.csproj" />
    <ProjectReference Include="..\Notifications\Notifications.Api\Notifications.Api.csproj" />
  </ItemGroup>
</Project>
```

Do **not** emit the 9.x form: no `<Project Sdk="Microsoft.NET.Sdk">` with a nested
`<Sdk Name="Aspire.AppHost.Sdk" Version="..." />`, no `<IsAspireHost>`, no explicit
`Aspire.Hosting.AppHost` package reference. The SDK adds that package implicitly.

**Central package management gotcha:** `Aspire.Hosting.AppHost` still needs a `PackageVersion`
entry in `Directory.Packages.props` even though nothing references it explicitly, because the
implicit reference has to resolve against a version.

**Version drift gotcha:** the SDK version in the `Sdk` attribute is MSBuild, not NuGet. NuGet
tooling will not update it. If it drifts from the `Aspire.Hosting.*` package versions, startup
fails with `Newer version of Aspire.Hosting.AppHost required`. Keep them in lockstep.

---

## 3. Solution structure

```
EventSourcingLab.sln
Directory.Build.props          net10.0, nullable, implicit usings, CPM on
Directory.Packages.props       all versions, centrally managed
.editorconfig
README.md
tools/seed.sh
src/
  AppHost/                     Aspire orchestration
  Shared/
    Contracts/                 published schema + envelope
    Messaging/                 transport abstraction + RabbitMQ + Kafka
    ServiceDefaults/           telemetry, health checks, service discovery
  Users/
    Users.Domain/
    Users.Application/
    Users.Infrastructure/
    Users.Api/
  Notifications/
    Notifications.Domain/
    Notifications.Application/
    Notifications.Infrastructure/
    Notifications.Api/
```

### Layering rules — enforced, not aspirational

Each service has exactly four layers. Dependencies point inward only:

```
Api  ->  Infrastructure  ->  Application  ->  Domain
```

- **Domain** — aggregates, events, value objects, invariants. **Zero package references.** If
  a Marten, Kafka or ASP.NET type is reachable from a domain project, the layering is broken.
- **Application** — commands, handlers, port interfaces, read model shapes. Orchestration only;
  no business rules. May reference Domain and Contracts.
- **Infrastructure** — Marten, projections, outbox, transport adapters, DI registration.
  Implements the Application's ports.
- **Api** — controllers and background consumers. Thin.

`Shared/Messaging` holds transport plumbing only (connect, publish, subscribe). Each service
adapts it to its **own** port interface inside its **own** Infrastructure layer, so the layering
rule survives the sharing.

### Solution file

Generate a classic `.sln` (not `.slnx`) with solution folders `Shared`, `Users`,
`Notifications`, and a `Solution Items` folder holding the root config plus the JSON schema.
Having both a `.sln` and a `.slnx` in one directory makes `dotnet build` fail with ambiguity.

Verify with `dotnet sln list` before claiming it works.

---

## 4. Users service — event sourced

### Why event sourcing here needs help

A plain user CRUD is a *bad* event sourcing example: almost no invariants, and you end up with
`UserUpdated` events that are CRUD in disguise. Fix this by giving the aggregate a **real state
machine**: contact verification.

### Key design move

Separate **verified** contact from **pending** contact. A new email does not replace the current
one until it is confirmed. This is what creates genuine states, transitions and failure modes.

### Domain model

```csharp
public sealed record PendingContact(
    string Value, string TokenHash, DateTimeOffset ExpiresAt, int FailedAttempts);

public sealed record NotificationPreferences(
    bool EmailEnabled, bool SmsEnabled, TimeOnly? QuietHoursStart, TimeOnly? QuietHoursEnd);

public enum VerificationFailureReason { TokenMismatch, TokenExpired, TooManyAttempts }
```

Aggregate `User` state: `Id`, `Name`, `VerifiedEmail`, `VerifiedPhone`, `PendingEmail`,
`PendingPhone`, `Preferences`, `IsActive`, `Version`.

### Commands

`RegisterUser`, `RequestEmailChange`, `VerifyEmail`, `RequestPhoneChange`, `VerifyPhone`,
`ChangePreferences`, `DeactivateUser`, `ReactivateUser`.

### Domain events

`UserRegistered`, `EmailChangeRequested`, `EmailVerified`, `EmailVerificationFailed`,
`PhoneChangeRequested`, `PhoneVerified`, `PhoneVerificationFailed`, `PreferencesChanged`,
`UserDeactivated`, `UserReactivated`.

All carry `OccurredAt`. All are **internal** — see §5.

### Invariants

1. A token that is expired or wrong cannot verify a contact
2. After 3 failed attempts the pending contact is invalidated; the flow restarts
3. Cannot request a change to the email already verified for that user
4. A deactivated user rejects all mutation commands
5. SMS cannot be enabled before a phone is verified
6. Quiet hours need both bounds or neither
7. **Email is unique across all users** — see below

### Aggregate shape: Decide / Apply split

Two groups of members, kept strictly apart:

- `Decide*(command, now)` — validates invariants, **returns** events, never mutates
- `Apply(event)` — mutates state from an event; Marten calls these when rehydrating

This matters because it guarantees the same `Apply` path runs both for a fresh command and for a
replay, so a rebuild cannot diverge from live behaviour.

### Cross-aggregate uniqueness — the hard part

Email uniqueness cannot live on the aggregate. Implement it as a **reservation document** whose
id *is* the normalized email, written in the **same Marten transaction** as the event append:

```csharp
session.Events.AppendOptimistic(userId, ct, @event);
session.Store(new EmailReservation { Id = normalizedEmail, UserId = userId });
await session.SaveChangesAsync(ct);   // 23505 unique violation => EmailAlreadyInUseException
```

Marten makes the document id the primary key, so Postgres enforces it. On a verified email
change, delete the old reservation and store the new one in the same transaction.

**Do not skip this.** It is the most instructive part of the exercise: it is exactly where event
sourcing is genuinely awkward, and the lab is less useful without it.

### Optimistic concurrency

Use `AppendOptimistic`, catch Marten's `ConcurrencyException`, surface it as a domain-level
`ConcurrencyConflictException`, map to HTTP 409.

### Token handling

Store only the **hash** on the event; return the plaintext token to the caller. A leaked event
stream must not enable account takeover. SHA-256 is fine here.

### Projections — deliberately two profiles

| Projection | Type | Lifecycle | Role in the benchmark |
|---|---|---|---|
| `UserProfile` | single-stream | Inline | cheap baseline, read-after-write consistent |
| `VerificationFunnel` | multi-stream, keyed by `yyyy-MM-dd` | Async | **the expensive one** |

`VerificationFunnel` aggregates across every user stream into shared day buckets, so Marten's
async daemon walks the whole event table single-threaded. This is the projection whose rebuild
time gets compared against a partition-parallel Kafka consumer. It exists for that reason —
do not simplify it away.

Fields: registrations, email changes requested, emails verified, failures split by reason, phone
changes requested, phones verified, deactivations, plus a computed conversion rate.

### API (controllers, not minimal APIs)

```
POST   /api/users                      register
POST   /api/users/{id}/email           request email change  -> returns token
POST   /api/users/{id}/email/verify    verify
POST   /api/users/{id}/phone           request phone change  -> returns token
POST   /api/users/{id}/phone/verify    verify
PUT    /api/users/{id}/preferences
POST   /api/users/{id}/deactivate
POST   /api/users/{id}/reactivate
GET    /api/users/{id}                 UserProfile
GET    /api/users/{id}/events          raw stream, for inspecting the log by hand
GET    /api/users/funnel               VerificationFunnel
```

Returning the token over HTTP is a lab affordance so the flow is drivable by curl. Comment it as
such.

Map exceptions: `DomainException` -> 422, `EmailAlreadyInUseException` -> 409,
`ConcurrencyConflictException` -> 409.

---

## 5. The integration contract

### Rule: domain events never leave the service

The Users aggregate emits ~10 event types. Exactly **one** integration event is published, and
only when *verified* contact state changes.

`EmailChangeRequested` must **not** be published — a downstream consumer has no business knowing
about an unverified address. The translation ratio is roughly 5 domain events to 1 integration
event, and that ratio is itself something to observe.

Why this matters: publishing domain events couples every consumer to your internal model. You
then cannot refactor an event without coordinating with every consuming team. Translate at the
boundary instead.

### Event-carried state transfer

The published message carries the **full current contact snapshot**, not a delta and not a bare
id. Three properties follow, and all three are load-bearing:

- **Idempotent** — re-applying changes nothing
- **Self-sufficient** — the consumer never calls back to the Users service
- **Compactable** — Kafka can keep only the latest per key without losing state

A delta (`phone changed to X`) breaks all three: lose one message and the replica is permanently
wrong.

### Envelope

```json
{
  "eventId": "uuid",
  "type": "UserContactUpdated",
  "aggregateId": "uuid",
  "version": 14,
  "occurredAt": "2026-08-20T14:03:22Z",
  "schemaVersion": 1,
  "data": {
    "userId": "uuid",
    "name": "Ana",
    "email": "ana@example.com",
    "phone": "+573001234567",
    "emailVerified": true,
    "phoneVerified": true,
    "active": true,
    "preferences": { "email": true, "sms": false,
                     "quietHoursStart": "22:00", "quietHoursEnd": "07:00" }
  }
}
```

Every envelope field earns its place: `eventId` for de-duplication, `version` for out-of-order
rejection, `type` for routing, `occurredAt` for lag measurement, `schemaVersion` for evolution.
`version` comes from the Marten stream version.

### Schema ownership

- The producer **owns** the schema; publish it as JSON Schema in `Shared/Contracts/schemas/`
- **No shared DTO assembly.** Each service writes its own classes. The duplication is
  intentional: a shared contracts DLL couples deployment cycles and rebuilds the monolith.
- Set `additionalProperties: true` inside `data` so an old consumer tolerates new fields
- Consumers must ignore unknown fields on deserialize (`System.Text.Json` default — do not turn
  on `UnmappedMemberHandling.Disallow`)

### Evolution rules

**Compatible, ship freely:** add an optional field with a default; add a new event type; relax a
validation.

**Breaking, never in place:** remove, rename, retype, or change the meaning of a field; make an
optional field required.

For a breaking change use **expand / migrate / contract**: publish old and new side by side, let
each consumer migrate on its own release cadence (this phase can run for months, and that is the
point), then delete the old field. If the change is too large, publish `v2` alongside `v1`.

**Document this asymmetry in the schema README:** with RabbitMQ, history is only what is in
flight, so only forward compatibility for new messages matters. With Kafka and long retention, a
consumer resetting to offset 0 must read events written under older schemas. Compatibility stops
being a courtesy and becomes structural. Kafka buys replay and charges in schema discipline.

---

## 6. Transactional outbox

Publish from an outbox, **never** from the command handler. Publishing directly loses messages
whenever the process dies between commit and publish.

Implementation:

- `OutboxMessage` document: id, topic, partition key, payload, created, dispatched, attempts
- A scoped buffer implementing the Application's `IIntegrationEventOutbox` port; handlers enqueue
- The repository flushes the buffer onto the live Marten session immediately before
  `SaveChangesAsync`, so events + reservation + outbox row commit atomically
- A `BackgroundService` polls undispatched rows, publishes, marks dispatched, counts attempts

### A trap to avoid, and to document in a comment

Poll on `DispatchedAt == null`. Do **not** poll on `WHERE seq > @lastSeen`.

Sequence numbers are assigned at *insert* time while rows become visible at *commit* time. A
transaction holding seq 104 can commit after one holding 105. A poller that reads `> 103`, sees
105, and advances its cursor will skip 104 **forever**, silently. Marking rows dispatched
sidesteps this entirely.

This is the same failure mode that makes naive polling of a Postgres event table unsafe for
projections, which is one of the real arguments for a log-based transport. Leave the explanatory
comment in the code — it is part of what the lab teaches.

---

## 7. Messaging abstraction

Keep it **thin**. Two interfaces:

```csharp
Task PublishAsync(string topic, string partitionKey, ReadOnlyMemory<byte> body, CancellationToken ct);

Task SubscribeAsync(string topic, string consumerGroup,
    Func<ReadOnlyMemory<byte>, CancellationToken, Task> handler, CancellationToken ct);
```

Do not abstract further. Offset resets, consumer groups and compaction have **no RabbitMQ
analogue**, and papering over that would hide exactly what is being measured. The leak is the
finding.

Select the implementation from configuration (`Messaging:Kind` = `RabbitMq` | `Kafka`).

### RabbitMQ implementation

Topic exchange, durable queue per consumer group, manual ack, bounded prefetch (~32), nack with
`requeue: false` on handler failure so a poison message cannot spin forever.

Comment what it structurally cannot do: a queue only receives messages published after it was
bound (no history); ack deletes the message (a consumer bug is unrecoverable); a new consumer
needs a separate bootstrap path.

### Kafka implementation

- **Create the topic compacted**: `cleanup.policy=compact`, ~6 partitions, short `segment.ms` so
  compaction is observable in a short lab session
- Key every message by `userId` — this pins a user's events to one partition (ordering per user)
  *and* is the key compaction dedupes on
- Producer: `EnableIdempotence`, `Acks.All`, small `LingerMs` for batching
- Consumer: `EnableAutoCommit = false`; commit only after the handler succeeds; on failure do not
  commit, so the offset stays and the message is retried
- Handle `null` values as tombstones

---

## 8. Notifications service — deliberately not event sourced

Two distinct pieces. Keep them separate; the separation is the lesson.

### Piece 1: the replica

`UserContact` — a plain document, not an aggregate. Holds only the fields this service uses:
name, email, phone, verified flags, active, opt-ins, quiet hours, `Version`, `UpdatedAt`.

Behaviour it *does* own:
- `CanReceive(channel)` — active AND opted in AND verified AND value present
- `IsQuietAt(localTime)` — must handle a window that wraps past midnight
- `NextSendableMoment(from)` — delay into the future rather than dropping

**Idempotency is version-based and is the whole trick:**

```csharp
if (existing is not null && snapshot.Version <= existing.Version) return;  // stale, drop
```

That single check makes redelivery, out-of-order arrival, and a full replay from offset 0 all
safe. Without it, replay corrupts the replica and the Kafka experiments are meaningless.

### Piece 2: the notification

`Notification` — a small aggregate with real rules and a state machine:

```
Scheduled -> Sending -> Sent
                |
                +-> Failed -> Scheduled (retry, exponential backoff)
                |
                +-> Discarded  (permanent error, or attempts exhausted)
```

Rules: refuse a channel the user cannot receive on; respect quiet hours by delaying; dedupe by
caller-supplied idempotency key (enforce with a Marten unique index); max 5 attempts; backoff
2s/4s/8s/16s/32s; distinguish transient from permanent failure.

Re-check `CanReceive` at send time, not only at schedule time — preferences may have changed in
between.

Sender: a fake implementation that fails a deterministic slice of traffic (~3% permanent, ~7%
transient) so retry and discard paths actually get exercised during benchmarks.

### Consumer

A `BackgroundService` subscribing to `users.contact` as group `notifications`. It deserializes
into its **own** classes, maps to a local snapshot type, and calls the handler. Log a warning
(do not crash) on a `schemaVersion` higher than it understands.

### API

```
POST /api/notifications                    request a notification
GET  /api/notifications/{id}
GET  /api/notifications/replica/status     count of replicated users
GET  /api/notifications/replica/{userId}
```

The replica count endpoint exists so a user can compare it against the Users service and see at a
glance whether the transport actually delivered everything.

---

## 9. AppHost

Resources: Postgres (with pgAdmin, data volume) exposing `usersdb` and `notificationsdb`;
RabbitMQ (management plugin, data volume); Kafka (Kafka UI, data volume). Both transports start
so the switch requires no infrastructure change.

A single `const string transportKind` at the top selects the transport. Both services get
`Messaging__Kind` and `Messaging__ConnectionString`.

**Critical:** pass the connection string as the resource builder, not as a formatted string:

```csharp
IResourceBuilder<IResourceWithConnectionString> messaging =
    transportKind == "Kafka" ? kafka : rabbit;

.WithEnvironment("Messaging__ConnectionString", messaging)   // correct
```

A hand-built `$"{{{name}.connectionString}}"` is **not** resolved; the service starts with a
literal placeholder and fails to connect.

Use `WithReference(...).WaitFor(...)` for ordering, and `WithHttpHealthCheck("/health")`.

### Launch profiles

Give both APIs **fixed ports** in `Properties/launchSettings.json` — Users `:5100`,
Notifications `:5200` — so the seed script works as documented instead of chasing dynamic ports.
Give the AppHost a launch profile with dashboard and OTLP endpoint URLs.

---

## 10. Tooling

`tools/seed.sh <count> [usersApi] [concurrency]` driving the real HTTP API:

1. Register a user
2. Request a phone change, capture the returned token
3. Fail verification deliberately ~20% of the time (so the funnel has real numbers), then succeed
4. Enable SMS (only legal once the phone is verified)
5. Deactivate ~10%

Parallelize with `xargs -P`. Print elapsed time and the two comparison commands at the end.

For large seeds, batch `AppendEvent` in transactions of ~1000 via a CLI command rather than HTTP.
Reserve a real load tool (k6, NBomber) for live traffic while measuring latency.

---

## 11. Experiments the code must enable

1. **Projection rebuild** — time a full `VerificationFunnel` rebuild via the Marten daemon vs. a
   Kafka consumer group doing the same aggregation across partitions.
2. **Write-side impact** — command latency percentiles *while* a rebuild runs. This is where the
   log-based path wins in a way a throughput chart will not show.
3. **Bootstrap** — wipe the notifications database and restart. Kafka with a compacted topic
   recovers every user by reading from offset 0. RabbitMQ has only what arrived after binding.
   **This is the single most convincing difference in the lab.**
4. **Reprocess after a bug** — change the handler, wipe the replica, recover. Kafka: reset the
   consumer group offset. RabbitMQ: impossible without producer cooperation.
5. **Consumer downtime** — stop Notifications, seed thousands of users, restart. Both survive,
   but watch RabbitMQ queue depth and memory: sustained backlog triggers flow control, which
   throttles the **producer**. The Users service degrades because a downstream consumer is down.

### Measurement discipline

Measure end-to-end latency p50/p95/p99 (commit to replica visible), full rebuild time, command
latency during a rebuild, and scaling across 1/2/4 projector instances — not just events/second.

**Both projection paths must write to the same destination shape.** If one writes to Marten and
the other to Redis, the benchmark measures storage engines, not transports. This is the most
common way this kind of comparison gets invalidated.

---

## 12. Non-negotiables checklist

- [ ] Domain projects have zero package references
- [ ] Decide methods return events and never mutate; Apply methods mutate and never validate
- [ ] Email uniqueness enforced by a reservation row in the same transaction as the append
- [ ] `AppendOptimistic` used; concurrency conflicts surface as 409
- [ ] Only verification tokens' **hashes** are persisted
- [ ] Exactly one integration event type is published; domain events stay internal
- [ ] Integration event carries full state, keyed by user id
- [ ] Outbox commits in the same transaction as the events
- [ ] Outbox polls on dispatched-flag, not on a sequence cursor, with the comment explaining why
- [ ] Consumer drops snapshots at or below the stored version
- [ ] Kafka topic created with `cleanup.policy=compact`
- [ ] Notifications replica is **not** event sourced
- [ ] Both transports implemented behind the same two-method interface
- [ ] AppHost uses the Aspire 13 SDK form and passes connection strings as resource builders
- [ ] Fixed API ports so `tools/seed.sh` works unmodified
- [ ] `dotnet sln list` shows all 12 projects; `dotnet build` succeeds

## 13. Verification

```bash
dotnet sln list      # 12 projects
dotnet build         # compiles without containers
dotnet run --project src/AppHost/AppHost.csproj
```

Then, with the app running:

```bash
./tools/seed.sh 500
curl -s localhost:5100/api/users/funnel
curl -s localhost:5200/api/notifications/replica/status   # should converge on 500
```

A README must document: how to run (CLI and IDE), prerequisites (.NET 10 SDK, a container
runtime), how to switch transports, the design decisions above, the experiments, and a
troubleshooting section covering Aspire SDK version drift and the connection-string placeholder
failure.
