namespace WebSpark.SharedKernel.DomainEvents;

/// <summary>
/// A simple interface for sending domain events. Can use MediatR or any other implementation.
/// Provides a contract for dispatching domain events and clearing them from entities after processing.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches all domain events from the provided entities and clears them after processing.
    /// This method should be called after entities have been persisted to ensure events are only
    /// dispatched for successfully saved changes.
    /// </summary>
    /// <param name="entitiesWithEvents">The entities containing domain events to dispatch.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DispatchAndClearEvents(IEnumerable<IHasDomainEvents> entitiesWithEvents);
}
