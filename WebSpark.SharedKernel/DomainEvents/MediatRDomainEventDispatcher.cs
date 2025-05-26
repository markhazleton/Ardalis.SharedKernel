using MediatR;
using Microsoft.Extensions.Logging;
using WebSpark.SharedKernel.Entities;

namespace WebSpark.SharedKernel.DomainEvents;

/// <summary>
/// MediatR-based implementation of IDomainEventDispatcher.
/// Publishes domain events via MediatR and provides comprehensive logging.
/// </summary>
public class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<MediatRDomainEventDispatcher> _logger;

    /// <summary>
    /// Initializes a new instance of the MediatRDomainEventDispatcher class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator for publishing events.</param>
    /// <param name="logger">The logger for recording dispatch operations.</param>
    public MediatRDomainEventDispatcher(IMediator mediator, ILogger<MediatRDomainEventDispatcher> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task DispatchAndClearEvents(IEnumerable<IHasDomainEvents> entitiesWithEvents)
    {
        foreach (IHasDomainEvents entity in entitiesWithEvents)
        {
            if (entity is HasDomainEventsBase hasDomainEvents)
            {
                DomainEventBase[] events = hasDomainEvents.DomainEvents.ToArray();
                hasDomainEvents.ClearDomainEvents();

                foreach (DomainEventBase domainEvent in events)
                    await _mediator.Publish(domainEvent).ConfigureAwait(false);
            }
            else
            {
                _logger.LogError(
                  "Entity of type {EntityType} does not inherit from {BaseType}. Unable to clear domain events.",
                  entity.GetType().Name,
                  nameof(HasDomainEventsBase));
            }
        }
    }
}
