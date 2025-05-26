# API Reference

This document provides comprehensive API reference for WebSpark.SharedKernel.

## Core Entities

### EntityBase

Base class for entities with integer ID.

```csharp
public abstract class EntityBase : HasDomainEventsBase
{
    public int Id { get; set; }
}
```

### EntityBase&lt;TId&gt;

Base class for entities with strongly-typed ID.

```csharp
public abstract class EntityBase<TId> : HasDomainEventsBase
    where TId : struct, IEquatable<TId>
{
    public TId Id { get; set; }
}
```

**Type Parameters:**

- `TId` - The type of the entity identifier. Must be a struct that implements IEquatable.

### EntityBase&lt;T, TId&gt;

Base class for entities with custom ID types (useful with Vogen).

```csharp
public abstract class EntityBase<T, TId> : HasDomainEventsBase
    where T : EntityBase<T, TId>
{
    public TId Id { get; set; }
}
```

**Type Parameters:**

- `T` - The entity type (for self-referencing constraint)
- `TId` - The type of the entity identifier

### HasDomainEventsBase

Provides domain event functionality to entities.

```csharp
public abstract class HasDomainEventsBase : IHasDomainEvents
{
    public IReadOnlyCollection<DomainEventBase> DomainEvents { get; }
    protected void RegisterDomainEvent(DomainEventBase domainEvent);
    internal void ClearDomainEvents();
}
```

**Properties:**

- `DomainEvents` - Read-only collection of registered domain events

**Methods:**

- `RegisterDomainEvent(DomainEventBase domainEvent)` - Registers a domain event with the entity
- `ClearDomainEvents()` - Clears all registered domain events (internal use)

## Interfaces

### IAggregateRoot

Marker interface for aggregate root entities.

```csharp
public interface IAggregateRoot { }
```

Apply this interface only to aggregate root entities. Repository implementations can use constraints to ensure they only operate on aggregate roots.

### IHasDomainEvents

Interface for entities that can raise domain events.

```csharp
public interface IHasDomainEvents
{
    IReadOnlyCollection<DomainEventBase> DomainEvents { get; }
}
```

**Properties:**

- `DomainEvents` - Read-only collection of domain events registered with the entity

## Value Objects

### ValueObject

Abstract base class for implementing value objects with proper equality semantics.

```csharp
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();
    public override bool Equals(object? obj);
    public bool Equals(ValueObject? other);
    public override int GetHashCode();
}
```

**Key Features:**

- Structural equality based on component values
- Hash code generation based on equality components
- Proxy-aware equality for Entity Framework Core compatibility

**Abstract Methods:**

- `GetEqualityComponents()` - Returns the components used for equality comparison

### PersonName

Example value object representing a person's name.

```csharp
public sealed class PersonName : ValueObject
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string FullName { get; }
    public string ReverseName { get; }
    public string SingleInitials { get; }
    public string ComplexInitials { get; }
    
    public PersonName(string first, string last);
    public override string ToString();
}
```

**Properties:**

- `FirstName` - The first name
- `LastName` - The last name
- `FullName` - Full name in "FirstName LastName" format
- `ReverseName` - Name in "LastName, FirstName" format
- `SingleInitials` - Simple initials from first letters
- `ComplexInitials` - Complex initials using first three characters

## Domain Events

### DomainEventBase

Abstract base class for all domain events.

```csharp
public abstract class DomainEventBase : INotification
{
    public DateTime DateOccurred { get; protected set; }
}
```

**Properties:**

- `DateOccurred` - UTC timestamp when the event was created

**Features:**

- Implements MediatR's INotification interface
- Automatically sets DateOccurred on creation

### IDomainEventDispatcher

Interface for dispatching domain events.

```csharp
public interface IDomainEventDispatcher
{
    Task DispatchAndClearEvents(IEnumerable<IHasDomainEvents> entitiesWithEvents);
}
```

**Methods:**

- `DispatchAndClearEvents(IEnumerable<IHasDomainEvents> entitiesWithEvents)` - Dispatches events and clears them from entities

### MediatRDomainEventDispatcher

MediatR-based implementation of IDomainEventDispatcher.

```csharp
public class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
    public MediatRDomainEventDispatcher(IMediator mediator, ILogger<MediatRDomainEventDispatcher> logger);
    public async Task DispatchAndClearEvents(IEnumerable<IHasDomainEvents> entitiesWithEvents);
}
```

**Features:**

- Publishes events via MediatR
- Comprehensive logging
- Automatic event clearing
- Error handling for non-conforming entities

## Repository Patterns

### IRepository&lt;T&gt;

Full repository interface for aggregate root entities.

```csharp
public interface IRepository<T> : IRepositoryBase<T> 
    where T : class, IAggregateRoot
{
}
```

Extends WebSpark.Specification's IRepositoryBase with aggregate root constraints.

**Available Operations from IRepositoryBase:**

- `Task<T> AddAsync(T entity)`
- `Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)`
- `Task<T?> GetByIdAsync<TId>(TId id)`
- `Task<T?> GetBySpecAsync(ISpecification<T> specification)`
- `Task<List<T>> ListAsync(ISpecification<T> specification)`
- `Task UpdateAsync(T entity)`
- `Task UpdateRangeAsync(IEnumerable<T> entities)`
- `Task DeleteAsync(T entity)`
- `Task DeleteRangeAsync(IEnumerable<T> entities)`
- `Task<int> CountAsync(ISpecification<T> specification)`
- `Task<bool> AnyAsync(ISpecification<T> specification)`

### IReadRepository&lt;T&gt;

Read-only repository interface for aggregate root entities.

```csharp
public interface IReadRepository<T> : IReadRepositoryBase<T> 
    where T : class, IAggregateRoot
{
}
```

Useful for query services and read-only operations in CQRS scenarios.

## CQRS Pattern Support

### ICommand&lt;TResponse&gt;

Interface for commands that modify state and return a response.

```csharp
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
```

### ICommandHandler&lt;TCommand, TResponse&gt;

Interface for command handlers.

```csharp
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}
```

### IQuery&lt;TResponse&gt;

Interface for queries that return data without modifying state.

```csharp
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
```

### IQueryHandler&lt;TQuery, TResponse&gt;

Interface for query handlers.

```csharp
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
```

## Behaviors

### LoggingBehavior&lt;TRequest, TResponse&gt;

MediatR pipeline behavior for comprehensive request logging.

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public LoggingBehavior(ILogger<Mediator> logger);
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct);
}
```

**Features:**

- Request/response logging
- Execution time tracking
- Property value inspection via reflection
- Configurable log levels

## Utilities

### SafeDictionary&lt;TKey, TValue&gt;

Type-safe, null-safe dictionary wrapper.

```csharp
public sealed class SafeDictionary<TKey, TValue> where TKey : notnull
{
    public SafeDictionary();
    public List<string> GetList();
    public TValue? GetValue(TKey key);
    public void SetValue(Dictionary<TKey, TValue> value);
    public void SetValue(TKey key, TValue value);
}
```

**Key Features:**

- Null-safe operations
- Convenient access methods
- Type safety enforcement

**Methods:**

- `GetList()` - Returns string representations of all key-value pairs
- `GetValue(TKey key)` - Gets value or default if not found
- `SetValue(Dictionary<TKey, TValue> value)` - Replaces entire dictionary
- `SetValue(TKey key, TValue value)` - Sets or updates a single key-value pair

## Usage Examples

### Creating an Entity

```csharp
public class Customer : EntityBase, IAggregateRoot
{
    public PersonName Name { get; private set; }
    public Email Email { get; private set; }

    public Customer(PersonName name, Email email)
    {
        Name = name;
        Email = email;
        RegisterDomainEvent(new CustomerCreatedEvent(Id, name.FullName));
    }

    public void UpdateEmail(Email newEmail)
    {
        if (!Email.Equals(newEmail))
        {
            Email = newEmail;
            RegisterDomainEvent(new CustomerEmailChangedEvent(Id, newEmail.Value));
        }
    }
}
```

### Creating a Value Object

```csharp
public class Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty");
        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format");
        Value = value.ToLowerInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    private static bool IsValidEmail(string email)
    {
        // Email validation logic
        return email.Contains('@') && email.Contains('.');
    }
}
```

### Creating a Domain Event

```csharp
public class CustomerCreatedEvent : DomainEventBase
{
    public int CustomerId { get; }
    public string CustomerName { get; }

    public CustomerCreatedEvent(int customerId, string customerName)
    {
        CustomerId = customerId;
        CustomerName = customerName;
    }
}
```

### Implementing a Repository

```csharp
public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(Email email);
    Task<IEnumerable<Customer>> GetActiveCustomersAsync();
}

public class CustomerRepository : EfRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Customer?> GetByEmailAsync(Email email)
    {
        return await GetBySpecAsync(new CustomerByEmailSpec(email));
    }

    public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
    {
        return await ListAsync(new ActiveCustomersSpec());
    }
}
```

### CQRS Implementation

```csharp
// Command
public record CreateCustomerCommand(string FirstName, string LastName, string Email) 
    : ICommand<int>;

// Command Handler
public class CreateCustomerHandler : ICommandHandler<CreateCustomerCommand, int>
{
    private readonly ICustomerRepository _repository;

    public CreateCustomerHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var name = new PersonName(request.FirstName, request.LastName);
        var email = new Email(request.Email);
        var customer = new Customer(name, email);
        
        await _repository.AddAsync(customer);
        return customer.Id;
    }
}

// Query
public record GetCustomerQuery(int Id) : IQuery<CustomerDto>;

// Query Handler
public class GetCustomerHandler : IQueryHandler<GetCustomerQuery, CustomerDto>
{
    private readonly IReadRepository<Customer> _repository;

    public GetCustomerHandler(IReadRepository<Customer> repository)
    {
        _repository = repository;
    }

    public async Task<CustomerDto> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.Id);
        return customer != null 
            ? new CustomerDto(customer.Id, customer.Name.FullName, customer.Email.Value)
            : throw new CustomerNotFoundException(request.Id);
    }
}
```

## Integration Examples

### Entity Framework Core Configuration

```csharp
public class ApplicationDbContext : DbContext
{
    private readonly IDomainEventDispatcher _dispatcher;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, 
                               IDomainEventDispatcher dispatcher) : base(options)
    {
        _dispatcher = dispatcher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entities
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.OwnsOne(c => c.Name, name =>
            {
                name.Property(n => n.FirstName).HasMaxLength(50);
                name.Property(n => n.LastName).HasMaxLength(50);
            });
            
            entity.OwnsOne(c => c.Email, email =>
            {
                email.Property(e => e.Value).HasMaxLength(255);
            });
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToArray();

        var result = await base.SaveChangesAsync(cancellationToken);
        
        await _dispatcher.DispatchAndClearEvents(entitiesWithEvents);
        
        return result;
    }
}
```

### Dependency Injection Setup

```csharp
// Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // MediatR
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    
    // Domain Event Dispatcher
    services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
    
    // Pipeline Behaviors
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    
    // Repositories
    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
    services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));
    services.AddScoped<ICustomerRepository, CustomerRepository>();
}
```
