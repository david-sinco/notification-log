# Contracts

Each JSON Schema here is a **published contract** owned by its producing service. There is
deliberately no shared DTO assembly: every service writes its own classes against a schema and
ignores fields it doesn't recognize (`System.Text.Json` default — never turn on
`UnmappedMemberHandling.Disallow`). A shared contracts DLL would couple both services' deployment
cycles; the duplication here is the price paid to avoid that.

## Evolution rules

**Compatible, ship freely:** add an optional field with a default; add a new event type; relax a
validation.

**Breaking, never in place:** remove, rename, retype, or change the meaning of a field; make an
optional field required. Use expand / migrate / contract instead: publish old and new fields side
by side, let each consumer migrate on its own release cadence, then delete the old field. If the
change is too large, publish a `v2` type alongside `v1`.

## The RabbitMQ / Kafka asymmetry

With RabbitMQ, history is only what is currently in flight — a consumer only ever sees messages
published after it bound its queue, so **only forward compatibility for new messages matters**.

With Kafka and long retention, a consumer that resets to offset 0 must be able to read events
written under older schemas too. Compatibility stops being a courtesy and becomes structural:
Kafka buys replay, and charges for it in schema discipline.
