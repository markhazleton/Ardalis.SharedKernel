namespace Ardalis.SharedKernel.DomainEvents;

public interface IHasDomainEvents
{
  IReadOnlyCollection<DomainEventBase> DomainEvents { get; }
}
