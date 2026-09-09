using Domain.Shared.EventSourcing;

namespace Domain.Shared.Common;

/// <summary>
/// Raíz de agregado. Sirve para los dos estilos que conviven en la solución sin que ninguno
/// pague por el otro:
/// </summary>
/// <remarks>
/// <para>
/// <b>Agregado clásico</b> (Notification, Recipient, Template, Trigger): el estado se guarda en
/// una fila y los eventos solo sirven para avisar a terceros. No sobrescribe <see cref="Apply"/>,
/// así que <see cref="Raise"/> se limita a encolar el evento y el estado lo cambian los métodos
/// de negocio a mano, como hasta ahora.
/// </para>
/// <para>
/// <b>Agregado event-sourced</b> (User): el estado no se guarda en ninguna parte, se deduce de
/// los eventos. Sobrescribe <see cref="Apply"/> y esa es toda la diferencia: <see cref="Raise"/>
/// pasa por ahí antes de encolar, y <see cref="Rehydrate{TAggregate}"/> pasa por ahí sin encolar.
/// </para>
/// <para>
/// La clase no conoce Marten ni EF a propósito. La versión del flujo tampoco vive aquí: es un
/// dato del store, no del dominio, y Marten la lleva por su cuenta al escribir.
/// </para>
/// </remarks>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot() { }

    protected AggregateRoot(Guid id) : base(id) { }

    /// <summary>
    /// Eventos decididos en esta unidad de trabajo y todavía sin guardar. El repositorio los
    /// vuelca al store y después llama a <see cref="ClearDomainEvents"/>.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Registra una decisión de negocio: cambia el estado y deja el evento pendiente de guardar.
    /// Se aplica <em>antes</em> de encolarlo para que un evento que el agregado no sepa manejar
    /// reviente aquí y no acabe escrito en el store, donde ya no habría vuelta atrás.
    /// </summary>
    protected void Raise(IDomainEvent domainEvent)
    {
        Apply(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Única transición de estado del agregado, y el único método que hace falta para releer
    /// historia: rehidratar es aplicar sin encolar. Debe ser total y sin efectos secundarios —
    /// se ejecuta tanto al decidir como al releer eventos de hace años, así que no puede
    /// rechazar nada por reglas de negocio (una regla que hoy es más estricta que ayer
    /// invalidaría el pasado) ni llamar a servicios externos.
    /// </summary>
    /// <remarks>
    /// Es pública porque quien rehidrata vive en otro ensamblado (el repositorio o la proyección
    /// de Marten) y C# no tiene una visibilidad intermedia. La implementación vacía es la de los
    /// agregados clásicos, que no derivan su estado de eventos.
    /// </remarks>
    public virtual void Apply(IDomainEvent domainEvent) { }

    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Reconstruye un agregado event-sourced a partir de su historia. Está aquí y no en cada
    /// agregado porque el algoritmo — instancia vacía, aplicar en orden — es siempre el mismo.
    /// </summary>
    /// <param name="id">
    /// Identidad del agregado, que <b>viene del flujo y no de los eventos</b>: el id del stream
    /// es el único dato de identidad, y repetirlo dentro de cada evento solo abre la puerta a
    /// que un payload contradiga al store.
    /// </param>
    public static TAggregate Rehydrate<TAggregate>(Guid id, IEnumerable<IDomainEvent> history)
        where TAggregate : AggregateRoot
    {
        ArgumentNullException.ThrowIfNull(history);

        // nonPublic para poder usar el constructor privado sin parámetros del agregado: así no
        // hace falta exponer uno público, que permitiría construir agregados inválidos desde
        // cualquier parte. Es el único punto de la clase que usa reflexión, y a cambio ningún
        // agregado tiene que escribir su propio Rehydrate.
        var aggregate = (TAggregate)Activator.CreateInstance(typeof(TAggregate), nonPublic: true)!;
        aggregate.Id = id;

        foreach (var domainEvent in history)
            aggregate.Apply(domainEvent);

        return aggregate;
    }
}
