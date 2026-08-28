# Event Driven Lab — the log as the database

A second, parallel example to [`../event-sourcing/`](../event-sourcing/README.md), built to make
Martin Kleppmann's "Turning the Database Inside Out" idea concrete using the same business domain
(user registration + email/phone verification; a Notifications service that reacts to it).

The difference in one sentence: **`event-sourcing/` has two services, each with its own private
database, that talk to each other over a broker; this solution has one shared, durable, replayable
Kafka log, and every service — including Users itself — rebuilds its own state by folding that log
from the beginning.** There is no Marten, no Postgres, no outbox, and no RabbitMQ here. The log
*is* the database.

## Status

All 12 projects build (`dotnet build EventDrivenLab.sln`). Runtime-verified against a real Kafka
broker: register a user, request an email change, verify it, confirm `GET /api/users/{id}`
reflects it, confirm Notifications' replica (`GET /api/notifications/replica/{userId}`) converges
independently by folding the same log, confirm a second user is correctly rejected with 409 when
trying to verify an email already owned by someone else, kill and restart both processes, and
confirm both rebuild completely from Kafka with nothing else durable — including that historical
data from before a restart reappears correctly.

Two real bugs surfaced during that runtime verification, both instructive enough to be worth
recording:

1. **A hosted BackgroundService subscribing to a topic must ensure the topic exists itself,** not
   rely on another materializer having done it first. `VerificationFunnelMaterializer` and
   `ContactMaterializer` originally skipped straight to `SubscribeAsync`, assuming
   `UserMaterializer`'s `EnsureTopicAsync` call would always run first — but hosted services start
   concurrently, not in a guaranteed order, and Notifications runs in a different process
   entirely. A consumer that queries topic metadata before the topic is properly created can end
   up assigned to a broker-auto-created topic with the wrong partition count and no retention
   override, and never notices. Every materializer now calls `EnsureTopicAsync` itself before
   subscribing — idempotent, so this costs nothing once the topic is already there.
2. **`TryGet` must return a copy, not the live reference, or the email-uniqueness check silently
   disables itself.** `UserCommandService` mutates the `User` it gets from `LoadAsync` via
   `User.Apply` *before* handing it to `AppendAsync` — that symmetry with a plain replay is the
   point (see `UserMaterializer`'s class comment). But `LoadAsync` was returning the same object
   instance stored in the materializer's own dictionary, so that mutation landed on the canonical
   state directly, and `AppendAsync`'s "did VerifiedEmail change relative to what's already
   stored?" check ended up comparing the object against itself — always equal, so a second user
   could silently steal an already-verified email. `UserMaterializer.TryGet` now returns a shallow
   clone (safe, since every `User` property is a primitive or an immutable record that Apply
   always *replaces*, never mutates in place).

## Why this design, concretely

`event-sourcing/`'s Users service already has a real event-sourced aggregate — this solution
reuses `Users.Domain` almost verbatim (same `User.Decide*`/`Apply` split, same commands, same ten
domain events). What changes is *where those events live* and *what "the source of truth" means*:

| | `event-sourcing/` | `event-driven/` |
|---|---|---|
| Source of truth | Postgres event store (Marten), per service | one shared Kafka topic, `users.events` |
| What crosses the service boundary | one translated integration event (`UserContactUpdated`) | the raw domain events themselves |
| Cross-aggregate invariant (email uniqueness) | a Postgres unique-index reservation row, same transaction as the append | an in-memory index folded from the same log, checked under a single-writer lock before producing |
| Outbox | yes — bridges the dual-write between Postgres and the broker | **none** — the log is the only store, so there's nothing to dual-write against |
| How a service's own state is built | Marten's inline/async projections over its own private event store | the service replays `users.events` itself, from offset 0, on every boot |
| How Notifications gets state | consumes one translated snapshot event, forever decoupled from Users' internal model | folds the *same* raw log Users writes — coupled to Users' wire shape, but with zero dependency on Users choosing to publish anything |

That last row is the actual point of this example: in `event-sourcing/`, Notifications
structurally *cannot* reconstruct itself without either asking Users or replaying the transport
from a compacted snapshot topic. Here, it can, from nothing but the log — because the log is
canonical for the whole system, not private to Users.

## The mechanics that make it correct

**No compaction.** `users.events` is created with `cleanup.policy=delete`, `retention.ms=-1`
(infinite), not compacted — the opposite of `event-sourcing/`'s `users.contact` topic. That topic
carries full snapshots, so "keep only the latest per key" is correct. This topic carries raw
events that must *all* survive to be fold-replayed; compaction would silently destroy the ability
to reconstruct anything. See `Users.Infrastructure/EventLog/UsersEventCodec.cs`.

**No outbox.** A successful Kafka produce ack *is* the commit — there's no second durable store
being written in the same transaction, so the dual-write problem the outbox pattern exists to
solve doesn't arise.

**One process-wide write lock, not one per user.** Email uniqueness is the one cross-aggregate
invariant in this domain. `event-sourcing/` enforces it with a Postgres unique index; here, there
is no database left to enforce it if two *different* users' commands race on the same email, so
`Users.Infrastructure/Materializer/UserMaterializer.cs` serializes every write command behind a
single `SemaphoreSlim`, and the Kafka produce await happens *inside* that lock. The honest cost:
every command — not just email verification — is serialized behind one Kafka round-trip. This is
a deliberate simplification tied to running exactly one `Users.Api` instance; a production version
of this idea (Kafka Streams, ksqlDB) gets the same single-writer-per-key guarantee from partition
assignment instead, allowing many partitions to make progress in parallel.

**Offset watermark instead of a version field.** `event-sourcing/`'s Notifications drops a
snapshot if `version <= existing.version`. There's no version field on this wire; the same job is
done once, structurally, by tracking the highest offset each partition has been applied through,
and skipping anything at or below it. This one mechanism absorbs redelivery, replay, and the race
between a command's own self-apply and the live consumer loop observing the same record — see
`UserMaterializer`'s own comments for exactly how.

**No shared DTOs, extended to a whole event union.** `event-sourcing/`'s Contracts project
publishes one schema for one translated event; each service still writes its own class for it.
Here, Notifications folds all ten raw Users event types, and still writes its own local record
shapes for them (`Notifications.Domain/RawUsersEvents.cs`) rather than referencing `Users.Domain`
— coupling to Users' wire shape is the accepted cost of this architecture, but the two codebases
stay independently deployable. Four of the ten types (the *requested*-but-not-yet-verified ones,
plus both verification-failure events) are irrelevant to a contact replica and are recognized and
skipped, not treated as an error — see `ContactMaterializer`.

**Fully in-memory, replayed from zero on every boot.** No SQLite, no embedded database — every
materializer (`UserMaterializer`, `VerificationFunnelMaterializer`, `ContactMaterializer`) rebuilds
its state entirely by replaying `users.events` from the true beginning each time the process
starts, using a fresh consumer-group id so a restart never resumes a stale committed offset. This
is the most direct possible demonstration of "the log is truth, the view is a disposable cache" —
and it's also this lab's version of `event-sourcing/SPEC.md §11`'s bootstrap experiment, except
here it's true of the *primary* write-side state, not just a replica. Each materializer logs how
many events it replayed and how long it took when it catches up, e.g.:

```
users.events per-user materializer replayed 1842 events in 340ms, caught up to the current high-watermark.
```

so "does this get slow forever as the topic grows" is a number you can watch, not a hidden worry.
`/health` stays unhealthy (and `InMemoryUserRepository`/`InMemoryUserContactRepository` block
individual requests) until that catch-up finishes.

## Solution layout

```
EventDrivenLab.sln
Directory.Build.props / Directory.Packages.props / .editorconfig
AppHost/                        Aspire orchestration — Kafka + Kafka UI + Mailpit only
Shared/
  Contracts/                    users.events envelope schema (no code)
  EventLog/                     IEventLog / KafkaEventLog — the log-as-database abstraction
  ServiceDefaults/               telemetry, health checks, service discovery (unchanged from event-sourcing/)
Web/                             Blazor Server hand-testing console (not part of the measured system,
                                  same role as event-sourcing/'s Web — calls both APIs over plain HTTP)
services/
  Users/
    Users.Domain/                reused near-verbatim from event-sourcing/
    Users.Application/           UserCommandService, no outbox
    Users.Infrastructure/        UserMaterializer, VerificationFunnelMaterializer, InMemoryUserRepository
    Users.Api/
  Notifications/
    Notifications.Domain/        Notification, UserContact (+ Apply overloads), RawUsersEvents
    Notifications.Application/   NotificationCommandService
    Notifications.Infrastructure/ ContactMaterializer, in-memory repositories, sender pipeline
    Notifications.Api/
```

## Running it

Prerequisites: .NET 10 SDK, a container runtime (Docker/Podman) for Kafka and Mailpit.

```bash
dotnet build EventDrivenLab.sln
dotnet run --project AppHost/AppHost.csproj
```

Fixed ports, distinct from `event-sourcing/`'s :5100/:5200/:5300 so both solutions can run side by
side: Users on **:5110**, Notifications on **:5210**, the Web console on **:5310**. The AppHost
dashboard/OTLP endpoints are likewise on their own ports (:17110/:17111 dashboard, :21110 OTLP) —
see `AppHost/Properties/launchSettings.json`. Mailpit's web UI is on **:8026** (not
event-sourcing/'s :8025).

Rather than curl, `http://localhost:5310` opens the same kind of Blazor Server test console
event-sourcing/ has — register/verify/inspect on the Users page, schedule/look-up on the
Notifications page, both against the real HTTP APIs. The two consoles differ only where the APIs
themselves differ: this one has no separate `UserProfile` read-model type (`GET /api/users/{id}`
already returns the folded state directly), its raw-event view has no top-level version/offset
column (just type + payload, in arrival order), and its replica view has no `Version` field (the
offset watermark plays that role here, invisibly).

```bash
# Register, then request + verify an email
curl -s -X POST localhost:5110/api/users -d '{"name":"Ana"}' -H 'content-type: application/json'
# -> note the returned "id"
curl -s -X POST localhost:5110/api/users/{id}/email -d '{"email":"ana@example.com"}' -H 'content-type: application/json'
# -> note the returned "token"
curl -s -X POST localhost:5110/api/users/{id}/email/verify -d '{"token":"..."}' -H 'content-type: application/json'

curl -s localhost:5110/api/users/{id}            # the folded state
curl -s localhost:5110/api/users/{id}/events     # the raw log for this user, kept alongside the fold
curl -s localhost:5110/api/users/funnel          # the day-bucket projection, independently materialized

curl -s localhost:5210/api/notifications/replica/status      # should converge on the same user count
curl -s localhost:5210/api/notifications/replica/{id}        # Notifications' own fold of the same log
```

**The experiment this example exists for:** stop the AppHost, start it again, and watch both
services' logs print their replay line and `/health` stay unhealthy until it does — then confirm
every user, the funnel, and the Notifications replica all reappear with nothing durable anywhere
except Kafka.
