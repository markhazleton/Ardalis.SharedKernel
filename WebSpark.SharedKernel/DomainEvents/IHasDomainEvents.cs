namespace WebSpark.SharedKernel.DomainEvents;

/// <summary>
/// Interface for entities that can raise domain events. 
/// Provides access to a collection of domain events that have been registered with the entity.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets a read-only collection of domain events that have been registered with this entity.
    /// </summary>
    IReadOnlyCollection<DomainEventBase> DomainEvents { get; }
}
