namespace Users.Application.Ports;

/// <summary>
/// A scoped buffer that command handlers enqueue onto. Users.Infrastructure's repository flushes
/// it onto the live Marten session immediately before SaveChangesAsync, so the appended domain
/// events, the email reservation, and the outbox row all commit in one transaction (SPEC.md §6).
/// </summary>
public interface IIntegrationEventOutbox
{
    void Enqueue(string topic, string partitionKey, object payload);
}
