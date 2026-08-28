# Schema ownership

Users owns `users.events` and publishes this schema. Consumers (Users' own materializers, and
Notifications' contact materializer) write their own local record types against it rather than
referencing a shared DTO assembly — the duplication is intentional, same rationale as
`event-sourcing/`'s contracts: a shared DLL would couple both services' deploy cycles.

The real difference from `event-sourcing/`: there, exactly one translated event
(`UserContactUpdated`) crosses the boundary, and it alone is schema'd. Here, the raw union of all
ten Users domain events is the shared artifact, because the whole point of this architecture is
that nothing gets translated at a boundary — every consumer reads what Users itself wrote. A
consumer must recognize an unfamiliar `type` value gracefully (log and skip), not crash — see
`Notifications.Infrastructure/EventLog/UsersEventCodec.cs`'s handling of the four event types it
doesn't care about.
