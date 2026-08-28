# Event Sourcing Lab

A lab for comparing Marten-driven vs. broker-driven projections, RabbitMQ vs. Kafka for
cross-service state replication, and event sourcing vs. plain persistence — side by side in one
solution. See [SPEC.md](SPEC.md) for the full design rationale.

## Status

All 12 projects are implemented and the whole system has been runtime-verified against real
Postgres and RabbitMQ (via `dotnet run --project AppHost/AppHost.csproj`): register → request
phone change → deliberately fail verification → verify → enable SMS → replicate to Notifications
→ schedule and send a notification, plus `tools/seed.sh` driving a batch of users through that
same flow and the funnel/replica-status endpoints converging on real numbers. Email sends are
also verified for real: the Email channel delivers through Mailpit, and an actual message with
the right from/to/subject/body shows up in its inbox.

- [x] Solution structure, `Users.*`, `Notifications.*`, `Shared/Messaging` (RabbitMQ + Kafka),
      `AppHost` (Postgres/RabbitMQ/Kafka/Mailpit resources, transport switch), `tools/seed.sh`,
      `Web` (a Blazor Server hand-testing console — see below)

**Not yet exercised:** the `Kafka` transport path (`transportKind` in `AppHost/Program.cs` has
only been run as `"RabbitMq"`) and the async `VerificationFunnelBucket` projection at a scale
large enough to matter for the rebuild-time experiment in SPEC.md §11. Both should work off the
same code paths already verified, but neither has been run for real.

Three real bugs surfaced during runtime verification that static review and compilation didn't
catch — worth knowing about if you extend this:

1. **Marten's self-aggregating convention needs an explicit `static T Create(FirstEvent)`.** A
   bare parameterless constructor plus `Apply(FirstEvent)` is silently never invoked for the event
   that starts a stream. `User` and `UserProfile` both have one.
2. **Every type Marten stores as a document (not just replays as events) needs public setters.**
   `LoadAsync`/inline-projection reads deserialize through JSON, and a private setter silently
   deserializes to the property's default instead of failing loudly — the symptom is a document
   that reads back as all-default values despite the correct data being in Postgres. Affects
   `User`, `UserProfile`, `VerificationFunnelBucket`, and `Notification`.
3. **A Marten session with the default identity map can't `Store()` a second, different instance
   under an Id it already `LoadAsync`'d in the same session** — it throws
   `InvalidOperationException: Document ... already added to the session`. This hit
   `ContactReplicationService`, which loads the existing `UserContact` to compare versions and
   then stores a freshly-mapped instance. `Notifications.Infrastructure` now registers Marten with
   `.UseLightweightSessions()` to avoid it. Combined with `RabbitMqTransport` nacking-without-
   logging by default, this failure was completely silent — a snapshot update would just vanish.
4. **An empty (but non-null) email/phone was accepted with no validation** and crashed three
   steps later, at *verification* time, with an unhandled 500 (`Id/id values cannot be null or
   empty`) when Marten tried to use it as the `EmailReservation` document id. `User.Decide*`
   now rejects an empty `Name`/`Email`/`Phone` at the point of the original command instead.
   Found via `Web`'s test console — the first real user of this API surface that wasn't curl with
   a hand-written, always-valid request body.

This section is updated as the system evolves; the rest of this README documents the parts that
exist today plus the plan for what doesn't yet.

## Solution layout

```
EventSourcingLab.sln
Directory.Build.props          net10.0, nullable, implicit usings, CPM on
Directory.Packages.props       all versions, centrally managed
.editorconfig
README.md
AppHost/                       Aspire orchestration
Shared/
  Contracts/                   published schema (schemas/user-contact-updated.schema.json)
  Messaging/                   transport abstraction (IMessageTransport) + RabbitMQ/Kafka impls
  ServiceDefaults/             telemetry, health checks, service discovery
Web/                           Blazor Server hand-testing console (not part of the measured system)
services/
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

Dependencies point inward only: `Api -> Infrastructure -> Application -> Domain`. Both `*.Domain`
projects have **zero package references** — if a Marten, Kafka, or ASP.NET type becomes reachable
from a Domain project, the layering is broken. `Web` sits outside that chain entirely: it's a
plain HTTP client of both `*.Api` projects (no project reference to either), placed at the root
next to `AppHost` because — like `AppHost` — it's cross-cutting infrastructure that spans both
services, not something that belongs to either one's `services/` subtree.

## The core insight being demonstrated

Event-driven architecture and event sourcing are different axes, not alternatives:

- **Event sourcing** describes how *one service stores its own state* (internal) — this is
  `Users`, whose aggregate has real invariants and a state machine for contact verification.
- **EDA** describes how *services communicate* (external) — `Notifications` never event-sources
  its own state; it replicates a plain document (`UserContact`) using last-write-wins keyed by
  version, and separately runs a small `Notification` aggregate for its own send/retry rules.

## Prerequisites

- .NET 10 SDK
- A container runtime (Docker Desktop or Podman) for Aspire's Postgres/RabbitMQ/Kafka/Mailpit
  resources

## Running

```bash
dotnet sln list      # 13 projects
dotnet build         # compiles without containers
dotnet run --project AppHost/AppHost.csproj
```

From an IDE: set `AppHost` as the startup project and run — Aspire's dashboard opens
automatically.

## Test console (Web)

`Web` is a small Blazor Server app for driving `Users.Api` and `Notifications.Api` by hand — the
same requests `tools/seed.sh` and curl would make, just with forms and buttons instead of typing
JSON. It's fixed at **http://localhost:5300** and has no other purpose: it's not part of what
SPEC.md §11 measures, doesn't sit in either service's dependency chain, and talks to both APIs
purely over HTTP+JSON with its own local copies of their request/response shapes — the same
"duplicate at the boundary" rule the two real services use with each other (SPEC.md §5), applied
here for the same reason: nothing here should couple to either service's internals.

It's genuinely useful for finding bugs the fixed, always-valid payloads in this repo's own tests
and scripts never would — see bug #4 above, found by leaving an email field blank in the UI.

## Verifying email delivery (Mailpit)

`AppHost` runs a [Mailpit](https://mailpit.axllent.org/) container as a fake SMTP server — no real
email ever leaves the machine. Its web UI is fixed at **http://localhost:8025**; every message a
notification actually sends shows up there in real time, so you can open it and read the exact
subject/body/recipient instead of trusting the fake sender's word for it.

How it fits together (`Notifications.Infrastructure/Sending/MailpitEmailSender.cs`): the fake
sender still makes the deterministic pass/fail/retry decision described in SPEC.md §8 — that's
what exercises the retry and discard paths. `MailpitEmailSender` decorates it: only when that
decision is "Sent" *and* the channel is Email does it also place a real SMTP call to Mailpit. If
Mailpit itself is unreachable, that call failing counts as a transient failure and the
notification retries, rather than being marked sent on a delivery that didn't happen. SMS has no
equivalent local capture tool, so it stays purely simulated.

`Smtp:Host` / `Smtp:Port` are only set when `AppHost` wires up Mailpit; running `Notifications.Api`
standalone without them falls back to the pure fake sender (see `appsettings.json`'s defaults,
which point at `localhost:1025` for a manually-run Mailpit instance).

```bash
curl -s -X POST localhost:5200/api/notifications \
  -H 'Content-Type: application/json' \
  -d '{"userId":"<a user id with a verified email>","channel":"Email","idempotencyKey":"demo-1","body":"hello"}'
# then open http://localhost:8025
```

## Switching transports

The transport is selected by a single `const string transportKind` at the top of
`AppHost/Program.cs`, set to `"RabbitMq"`, `"RabbitMqStream"`, or `"Kafka"`. Both services read it
from `Messaging:Kind` / `Messaging:ConnectionString`, injected by `AppHost` as the resource builder
for whichever transport is selected — every transport's infrastructure starts either way, so
switching never requires an infrastructure change, only editing that one constant and re-running.

### RabbitMQ Streams (`RabbitMqStream`)

A third transport, `Shared/Messaging/RabbitMqStreamTransport.cs`, using
[RabbitMQ Streams](https://www.rabbitmq.com/docs/streams) — a different sub-protocol of the same
broker `RabbitMqTransport` talks to over classic queues, not a setting on it. Where a classic queue
deletes a message on ack, a stream is a real append-only log: every publish is retained (capped at
`MaxLengthBytes`, currently 500MB), and any consumer can replay it from any offset. This is what
lets RabbitMQ do the "bootstrap a consumer from empty" and "replay after a handler bug" experiments
(SPEC.md §11) that classic queues structurally cannot — previously only Kafka could.

How it's wired: `AppHost` enables the `rabbitmq_stream`/`rabbitmq_stream_management` plugins on the
same `rabbitmq` container by bind-mounting `AppHost/rabbitmq/enabled_plugins` over the image's
default plugin list (`WithManagementPlugin` picks an image where only `rabbitmq_management` is
enabled by default; the mount adds streams on top, so both must be listed or management would
disappear). It also exposes the stream protocol's fixed port 5552 as a separate endpoint on that
same resource and passes it to both services as `Messaging__StreamPort` — `RabbitMqStreamTransport`
reuses the same AMQP connection string's host/credentials, just over this different port.

"Consumer group" maps onto `ConsumerConfig.Reference`: each reference (Notifications passes its own
consumer group name, same as it does for the other two transports) gets its own durable checkpoint
via `StreamSystem.StoreOffset`/`TryQueryOffset`, restored on reconnect — the direct analogue of a
Kafka committed offset, something classic RabbitMQ queues have no equivalent of at all. A brand new
reference with no stored offset starts from `OffsetTypeFirst`, replaying the whole retained log —
exactly Kafka's `Earliest`.

One structural difference from Kafka worth knowing: a stream here is never partitioned (no Super
Stream), so ordering within one stream is a *total* order across every key, not merely a
per-partition order the way Kafka's is — a stronger guarantee, traded for not being able to
parallelize reads across partitions the way Kafka's consumer groups do.

Runtime-verified end to end: registered a user, verified their email, and confirmed the resulting
`UserContactUpdated` arrived at `Notifications`' `/api/notifications/replica/{userId}` through a
real RabbitMQ Streams round trip (stream auto-created, offset checkpointed under the `notifications`
reference), then repeated it with a second update to confirm the same producer/consumer connections
are reused correctly rather than reconnecting per message.

### Outbox dispatch order

`Users.Infrastructure/Outbox/OutboxDispatcher.cs` only ever dispatches the **oldest undispatched
row per `PartitionKey`** in a given poll — if an older message for a user is still failing, a newer
message for that *same* user is never sent ahead of it, even though other users' messages keep
moving in the same pass. This matters once the transport itself preserves send order as the actual
log (RabbitMQ Streams' single stream, Kafka's per-partition log): publishing out of order there
would record the wrong order permanently, not just deliver it late. It was previously not
guaranteed — a failed message was skipped over, and only the receiving side's version check
(`ContactReplicationService`) absorbed the resulting reordering. That check is still there and
still necessary (it's what makes redelivery and replay-from-offset safe), but send order is now
also correct at the source, not just recoverable at the destination.

## Design decisions

See [SPEC.md](SPEC.md) for the full reasoning. In short:

- **Decide/Apply split** on the `User` aggregate: `Decide*` methods validate invariants and
  return events without mutating; `Apply` methods mutate from an event and are exactly what
  Marten calls on replay, so a rebuild can't diverge from live behaviour.
- **Pending vs. verified contact**: a new email/phone doesn't replace the current one until a
  token is confirmed. This is what gives the aggregate genuine states and failure modes instead
  of being CRUD in disguise.
- **Domain events never leave the service.** Exactly one integration event
  (`UserContactUpdated`) is published, carrying the full current contact snapshot — idempotent,
  self-sufficient, and compactable — not a delta.
- **Email uniqueness** is enforced by a reservation document (id = normalized email) written in
  the same Marten transaction as the verifying event, so Postgres's unique constraint does the
  enforcing.

## Experiments

The lab is meant to let you measure:

1. Projection rebuild time — Marten async daemon vs. a Kafka consumer group
2. Command latency percentiles while a rebuild runs
3. Bootstrap from empty — Kafka replays a compacted topic from offset 0; RabbitMQ only has what
   arrived after the queue was bound
4. Reprocessing after a handler bug — reset a Kafka consumer group offset vs. RabbitMQ's lack of
   an equivalent
5. Consumer downtime — RabbitMQ backlog triggers flow control that throttles the *producer*

## Troubleshooting

**`Newer version of Aspire.Hosting.AppHost required` on startup** — the `Sdk="Aspire.AppHost.Sdk/13.5.0"`
attribute in `AppHost.csproj` is an MSBuild version, not a NuGet one; NuGet tooling won't update
it automatically. Keep it in lockstep with the `Aspire.Hosting.*` package versions in
`Directory.Packages.props`.

**A service starts but can't connect to Postgres/RabbitMQ/Kafka** — check that `AppHost` passes
connection strings as resource builders (`.WithEnvironment("Messaging__ConnectionString",
messaging)`), not as a hand-built `$"{{{name}.connectionString}}"` string. The latter is not
resolved by Aspire and the service receives a literal placeholder.
