using System.ComponentModel.DataAnnotations.Schema;
using WebSpark.SharedKernel.DomainEvents;

namespace WebSpark.SharedKernel.Entities;

/// <summary>
/// Provides domain event functionality to entities. Implements the IHasDomainEvents interface
/// and manages a collection of domain events that can be registered and cleared.
/// </summary>
public abstract class HasDomainEventsBase : IHasDomainEvents
{
    private readonly List<DomainEventBase> _domainEvents = [];

    /// <summary>
    /// Gets a read-only collection of domain events registered for this entity.
    /// This property is marked as NotMapped to prevent Entity Framework from trying to persist it.
    /// </summary>
    [NotMapped]
    public IReadOnlyCollection<DomainEventBase> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Registers a domain event with this entity. The event will be dispatched after the entity is persisted.
    /// </summary>
    /// <param name="domainEvent">The domain event to register.</param>
    protected void RegisterDomainEvent(DomainEventBase domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Clears all registered domain events. This method is typically called by the domain event dispatcher
    /// after events have been processed.
    /// </summary>
    internal void ClearDomainEvents() => _domainEvents.Clear();
}
