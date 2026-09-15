using Domain.Shared.EventSourcing;

namespace Domain.Shared.Common;

public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot() { }

    protected AggregateRoot(Guid id) : base(id) { }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(IDomainEvent domainEvent)
    {
        Apply(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public virtual void Apply(IDomainEvent domainEvent) { }

    public void ClearDomainEvents() => _domainEvents.Clear();

    public static TAggregate Rehydrate<TAggregate>(Guid id, IReadOnlyCollection<IDomainEvent> history)
        where TAggregate : AggregateRoot
    {
        ArgumentNullException.ThrowIfNull(history);

        if (history.Count == 0)
            throw new ArgumentException(
                $"No se puede rehidratar {typeof(TAggregate).Name} '{id}': la historia está vacía. " +
                "Si el flujo no existe, el repositorio debe devolver null en lugar de rehidratar.",
                nameof(history));

        var aggregate = (TAggregate)Activator.CreateInstance(typeof(TAggregate), nonPublic: true)!;
        aggregate.Id = id;

        foreach (var domainEvent in history)
            aggregate.Apply(domainEvent);

        return aggregate;
    }
}
