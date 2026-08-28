# Event Sourcing Lab

Two independent solutions living side by side, both implementing the same business (user
registration + email/phone verification, plus a Notifications service that reacts to it), so the
same domain can be compared across two fundamentally different architectures.

- **[`event-sourcing/`](event-sourcing/README.md)** — Users is event-sourced on Marten/Postgres;
  Notifications is a plain last-write-wins replica fed by one translated integration event, over a
  switchable transport (RabbitMQ classic queues, RabbitMQ Streams, or Kafka). See
  `event-sourcing/SPEC.md` for the full build spec.

- **[`event-driven/`](event-driven/README.md)** — "Turning the Database Inside Out": a single
  Kafka log of raw domain events *is* the source of truth for the whole system. There is no
  private per-service database — every service, including Users itself, rebuilds its state by
  replaying the same shared log. No Marten, no outbox, no RabbitMQ.

Run either independently — different fixed ports, different Aspire dashboards, no shared
infrastructure. Each has its own `README.md` with setup and a curl walkthrough.
