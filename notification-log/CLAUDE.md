# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Scope

`notification-log/` is one of three independent solutions in the `EventSourcingLab` git repo (the
others are `event-sourcing/` and `event-driven/`, which share no code or infrastructure with this
one). The git root is the parent directory; this solution's root is `notification-log/`.

This is a teaching solution: a Notifications bounded context built with classic layered/DDD
architecture (no event sourcing here), plus a Blazor site that both administers it and hosts the
written tutorials about it.

**Everything user-facing is in Spanish** — code comments, XML docs, exception messages, API
summaries, UI text, and the tutorial pages. Match that when writing new code; the comments in this
codebase are long and explain *why* a decision was made, not what the line does. Preserve that
style.

## Commands

All commands run from `notification-log/`.

```powershell
dotnet build NotificationLog.slnx
dotnet run --project NotificationLog.AppHost      # runs EVERYTHING (see below)
dotnet test Services/Notification/NotificationLog.Tests
```

Always start the app through **AppHost**, never the individual projects — the API and Web read
their connection strings and service-discovery addresses from environment variables that only
Aspire injects. **Docker Desktop must be running**: AppHost starts SQL Server, Redis, RabbitMQ
(+ management plugin), DbGate, and Buggregator as containers.

Running a single test (the test project sets `EnableMSTestRunner`, so it is an executable using
Microsoft.Testing.Platform; args after `--` go to the test app):

```powershell
dotnet test Services/Notification/NotificationLog.Tests -- --filter "*GetWebResourceRootReturnsOkStatusCode*"
```

Tests are `Aspire.Hosting.Testing` integration tests — they boot the whole AppHost graph, so they
need Docker too and take tens of seconds each.

EF Core migrations (`NotificationDbContextFactory` supplies a design-time LocalDB connection, so no
`--startup-project` is needed):

```powershell
dotnet ef migrations add <Name> --project Services/Notification/NotificationLog.NotificationService.Infrastructure
```

Do not apply migrations by hand: `ApiService/Program.cs` calls `db.Database.MigrateAsync()` on
startup against the Aspire-provisioned SQL Server.

## Architecture

### Projects

- `NotificationLog.AppHost` — Aspire orchestrator; the single source of truth for topology, ports,
  and container wiring.
- `Services/Notification/*` — the Notification bounded context, in four layers:
  `Domain` → `Application` → `Infrastructure` → `ApiService`. Dependencies point inward only;
  `Domain` has zero package references.
- `Shared/NotificationLog.Contracts` — protobuf message contracts (`Grpc.Tools` generates the C#).
- `Shared/NotificationLog.ServiceDefaults` — the standard Aspire OpenTelemetry / health-check /
  service-discovery block, referenced by every runnable service.
- `NotificationLog.Web` — Blazor Server (interactive server render mode) admin UI + tutorials.

### Domain model (the four aggregates)

- **`NotificationTrigger`** — keyed by the `EventKey` value object (`dominio.accion`, lowercase,
  regex-enforced). Owns a collection of `NotificationConfiguration` children; the aggregate
  enforces "at most one *enabled* configuration per channel" and exposes
  `ResolveActiveConfigurations()`.
- **`NotificationTemplate`** — owns `TemplateVersion` children with an explicit "current version"
  concept; publishing a new version retires the previous one. A template is bound to one channel,
  and subject is only legal for Email/Push.
- **`Recipient`** — a **replica**, not a source of truth. Its `Id` *is* the upstream `user_id`;
  it is only ever written by `RecipientSyncService` from incoming messages (plus a dev-only
  endpoint). `CanReceive(channel)` / `AddressFor(channel)` decide deliverability.
- **`Notification`** — the immutable delivery log row (success or failure, per attempt).

Aggregates use factory methods + private setters + private backing collections; invariant
violations throw `DomainException`. `AggregateRoot` can `Raise` domain events, but nothing
currently dispatches them.

### Application layer

Hand-rolled CQRS — **no MediatR**. Each use case is a folder holding `XCommand` (record),
`XHandler` (plain class, registered explicitly in `Application/DependencyInjection.cs`), and
optionally `XValidator` (FluentValidation). Handlers call
`validator.ValidateAndThrowAppAsync(...)`, mutate aggregates through repository interfaces defined
in `Domain`, then `IUnitOfWork.SaveChangesAsync` (implemented by `NotificationDbContext`).

Two services in `Application` are *not* command handlers, because they are driven by messages
rather than HTTP, and fan out to several handlers:

- `NotificationDispatchService` — the core flow. Given a `BusinessEvent`: resolve trigger by event
  key → load recipient → for each active configuration, load template, render, send, then record
  success **or** failure via the corresponding command handler. Nothing is ever dropped silently:
  every render/send failure becomes a persisted failed `Notification`.
- `RecipientSyncService` — applies an inbound `UserChange` snapshot to the `Recipient` replica.

Neither is a `BackgroundService`; both are `Scoped` and invoked once per message inside the
consumer's scope.

### Messaging

**Wolverine over RabbitMQ**, configured in `Infrastructure/Messaging/RabbitMq/`. Two queues, each
pinned to a default incoming message type:

| Queue                   | Contract                        | Handler                       |
| ----------------------- | ------------------------------- | ----------------------------- |
| `user-changes`          | `UserContactUpdated`            | `UserContactUpdatedHandler`   |
| `notification-dispatch` | `NotificationDispatchRequested` | `NotificationDispatchHandler` |

There is **no real producer yet** — messages are published by hand from the RabbitMQ management UI.
That is why two serializers are registered: protobuf is the global default (content type
`binary/protobuf`), and System.Text.Json is added so a hand-typed JSON body with
`content_type: application/json` also deserializes. Wolverine picks by the AMQP `content_type`
property.

The consumers are static classes in `Infrastructure/Messaging/Consumers/` whose only job is to map
the protobuf message to the Application-layer record (`UserChange`, `BusinessEvent`) and call the
service — keep application code free of protobuf types.

`ServiceLocationPolicy.AlwaysAllowed` is set deliberately: repositories and validators are
`internal` and `IUnitOfWork` is registered via factory lambda, which Wolverine's generated code
cannot inline. Don't "fix" this by making those types public.

The two `.proto` files carry long comments about their semantics that matter when editing them:
`UserContactUpdated` is **event-carried state transfer** (a complete snapshot; empty string means
"no confirmed value", never "unchanged" — so no `optional` fields), while
`NotificationDispatchRequested` is a **generic envelope** any producer can send, with a free-form
`data` map that the configured template interprets.

### Rendering

`ScribanTemplateRenderer` builds a nested `ScriptObject`: recipient fields (and its attributes)
under `recipient.*`, and the event's `data` entries at the root. Example placeholders:
`{{ recipient.name }}`, `{{ order_number }}`. `StrictVariables = true` — an unmatched placeholder
throws rather than rendering empty, and the dispatch flow turns that into a recorded failure.

### Presentation

`ApiService` uses minimal APIs grouped per aggregate in `Endpoints/*.cs` (`MapTriggers()`,
`MapTemplates()`, …), each with `WithName`/`WithSummary`/`Produces` metadata for OpenAPI (Scalar UI
in Development). Request/response shapes live in `ApiService/Contracts/`, separate from the
Application DTOs.

Exceptions become RFC 7807 responses in `GlobalExceptionHandler`, and this mapping is the API's
error contract: `NotFoundException` → 404, `AppValidationException` → 400
(`ValidationProblemDetails`), `DomainException` → **422**, anything else → 500.

`NotificationLog.Web` never references the domain projects. It calls the API through four typed
clients (`Api/*ApiClient.cs`) whose base address is `https+http://apiservice`, resolved by Aspire
service discovery. Every client calls `response.EnsureSuccessAsync(ct)`, which parses the
ProblemDetails body and throws `ApiException` — the pages render that via `ErrorAlert`.

Besides the CRUD pages, `Components/Pages/{Proyecto,Ddd,Arquitectura}/` are long-form tutorial
pages. Each of those sections has its own tab-strip nav component in `Components/Ddd/`
(`ProyectoNav`, `DddNav`, `ArqNav`); when adding a page to a section, register it in the matching
nav component too.

### Dev-only infrastructure

AppHost adds two containers that are not Aspire integrations, so they are wired by raw environment
variables:

- **Buggregator** (`localhost:8000`) — single sink for outbound channels: SMTP on port 1025
  (`SmtpEmailNotificationSender`) and an SMS gateway on `/sms` (`HttpSmsNotificationSender`).
  `CLIENT_SUPPORTED_EVENTS=smtp,sms` disables its other modules, which also closes their ports.
- **DbGate** (`localhost:3000`) — browser SQL client pointed at the same SQL Server container.

Channel senders are configured per channel under `Notifications:{Email,Sms,Push}` in the API's
appsettings. WhatsApp has no provider: `NotImplementedWhatsAppNotificationSender` is registered
only so `NotificationDispatchService`'s constructor can be satisfied.
