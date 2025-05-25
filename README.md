# Ardalis.SharedKernel

[![NuGet](https://img.shields.io/nuget/v/Ardalis.SharedKernel.svg)](https://www.nuget.org/packages/Ardalis.SharedKernel/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Ardalis.SharedKernel.svg)](https://www.nuget.org/packages/Ardalis.SharedKernel/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive collection of base classes and utilities for implementing Domain-Driven Design (DDD) patterns in .NET applications. This package provides essential building blocks for clean architecture implementations, including entities, value objects, domain events, and repository abstractions.

## 🎯 Why Ardalis.SharedKernel?

Ardalis.SharedKernel simplifies the implementation of common DDD patterns by providing:

- **Type-safe entity base classes** with support for multiple ID types
- **Robust value object implementation** with proper equality semantics
- **Domain event infrastructure** with MediatR integration
- **Repository pattern abstractions** built on Ardalis.Specification
- **CQRS pattern support** with command and query abstractions
- **Comprehensive logging** for all MediatR requests

## 📦 Installation

```bash
dotnet add package Ardalis.SharedKernel
```

## 🚀 Quick Start

### 1. Create an Entity

```csharp
public class Customer : EntityBase, IAggregateRoot
{
    public string Name { get; private set; }
    public Email Email { get; private set; }

    public Customer(string name, Email email)
    {
        Name = Guard.Against.NullOrEmpty(name);
        Email = Guard.Against.Null(email);
        
        // Raise a domain event
        RegisterDomainEvent(new CustomerCreatedEvent(Id, name));
    }
}
```

### 2. Create a Value Object

```csharp
public class Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        Value = Guard.Against.NullOrEmpty(value);
        Guard.Against.InvalidFormat(value, nameof(value), @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### 3. Create a Domain Event

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

### 4. Implement a Repository

```csharp
public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(Email email);
}
```

## 📚 API Documentation

### Core Entities

#### `EntityBase`

The foundation for all domain entities in your application.

**Key Features:**

- Auto-incrementing integer ID
- Domain event support
- Three generic variants for different ID types

**Variants:**

```csharp
EntityBase                    // int Id
EntityBase<TId>              // Generic struct Id (Guid, long, etc.)
EntityBase<T, TId>           // For strongly-typed IDs (Vogen support)
```

**Usage:**

```csharp
// Standard integer ID
public class Order : EntityBase, IAggregateRoot { }

// GUID ID
public class Product : EntityBase<Guid>, IAggregateRoot { }

// Strongly-typed ID
public class Invoice : EntityBase<Invoice, InvoiceId>, IAggregateRoot { }
```

#### `HasDomainEventsBase`

Provides domain event functionality to entities.

**Methods:**

- `RegisterDomainEvent(DomainEventBase domainEvent)` - Adds a domain event
- `DomainEvents` - Read-only collection of pending events
- `ClearDomainEvents()` - Removes all pending events (internal use)

### Value Objects

#### `ValueObject`

Abstract base class for implementing value objects with proper equality semantics.

**Key Features:**

- Structural equality based on component values
- Hash code caching for performance
- Comparison support (IComparable)
- Proxy-aware equality (EF Core/NHibernate)

**Implementation:**

```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = Guard.Against.NullOrEmpty(currency);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

**💡 Modern Alternative:** For C# 10+, consider using `readonly record struct` for simple value objects:

```csharp
public readonly record struct Money(decimal Amount, string Currency);
```

### Domain Events

#### `DomainEventBase`

Abstract base class for all domain events.

**Properties:**

- `DateOccurred` - Automatically set to UTC timestamp on creation

**Integration:**

- Implements `INotification` from MediatR
- Dispatched after entity persistence

#### `IDomainEventDispatcher`

Contract for dispatching domain events.

**Implementation:**

```csharp
public async Task DispatchAndClearEvents(IEnumerable<IHasDomainEvents> entities)
{
    // Dispatch events and clear from entities
}
```

#### `MediatRDomainEventDispatcher`

Ready-to-use MediatR implementation of domain event dispatching.

**Features:**

- Publishes events via MediatR
- Comprehensive logging
- Automatic event clearing
- Error handling for non-conforming entities

### Repository Pattern

#### `IRepository<T>`

Full repository interface for aggregate roots.

**Inheritance:**

- Extends `IRepositoryBase<T>` from Ardalis.Specification
- Constraint: `T : class, IAggregateRoot`

**Available Operations:**

```csharp
// Create
Task<T> AddAsync(T entity);
Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

// Read
Task<T?> GetByIdAsync<TId>(TId id);
Task<T?> GetBySpecAsync(ISpecification<T> specification);
Task<List<T>> ListAsync(ISpecification<T> specification);

// Update
Task UpdateAsync(T entity);
Task UpdateRangeAsync(IEnumerable<T> entities);

// Delete
Task DeleteAsync(T entity);
Task DeleteRangeAsync(IEnumerable<T> entities);

// Queries
Task<int> CountAsync(ISpecification<T> specification);
Task<bool> AnyAsync(ISpecification<T> specification);
```

#### `IReadRepository<T>`

Read-only repository interface.

**Use Cases:**

- Query services
- Read-only operations
- CQRS query side

### CQRS Pattern Support

#### Commands

```csharp
// Command definition
public record CreateCustomerCommand(string Name, string Email) : ICommand<int>;

// Command handler
public class CreateCustomerHandler : ICommandHandler<CreateCustomerCommand, int>
{
    public async Task<int> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

#### Queries

```csharp
// Query definition
public record GetCustomerQuery(int Id) : IQuery<CustomerDto>;

// Query handler
public class GetCustomerHandler : IQueryHandler<GetCustomerQuery, CustomerDto>
{
    public async Task<CustomerDto> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### Interfaces

#### `IAggregateRoot`

Marker interface for aggregate root entities.

**Purpose:**

- Identifies aggregate boundaries
- Repository constraints
- Domain event origination

#### `IHasDomainEvents`

Contract for entities that can raise domain events.

**Properties:**

- `IReadOnlyCollection<DomainEventBase> DomainEvents`

### Behaviors

#### `LoggingBehavior<TRequest, TResponse>`

MediatR pipeline behavior for comprehensive request logging.

**Features:**

- Request/response logging
- Execution time tracking
- Property value inspection (via reflection)
- Configurable log levels

**Registration (Autofac example):**

```csharp
builder
    .RegisterGeneric(typeof(LoggingBehavior<,>))
    .As(typeof(IPipelineBehavior<,>))
    .InstancePerLifetimeScope();
```

## 🏗️ Architecture Integration

### Entity Framework Core

```csharp
public class ApplicationDbContext : DbContext
{
    private readonly IDomainEventDispatcher _dispatcher;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, 
                               IDomainEventDispatcher dispatcher) : base(options)
    {
        _dispatcher = dispatcher;
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

### Dependency Injection

```csharp
// Program.cs or Startup.cs
services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

## 🔧 Dependencies

This package relies on these well-established libraries:

- **[MediatR](https://github.com/jbogard/MediatR)** (12.5.0) - Mediator pattern implementation
- **[Ardalis.Specification](https://github.com/ardalis/Specification)** (9.1.0) - Repository pattern with specifications
- **[Ardalis.GuardClauses](https://github.com/ardalis/GuardClauses)** (5.0.0) - Guard clause extensions
- **[Microsoft.Extensions.Logging.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions)** (9.0.5) - Logging abstractions

## 📋 Requirements

- **.NET 8.0+** - Built with modern .NET features
- **C# 10+** - Utilizes recent language enhancements
- **Nullable reference types** - Enabled for better null safety

## 🏆 Best Practices

### 1. Create Your Own SharedKernel

> **Important:** This package is intended as a starting point and reference implementation. For production applications, create your own `YourCompany.SharedKernel` package with customizations specific to your domain.

### 2. Value Object Guidelines

- Use `readonly record struct` for simple value objects in C# 10+
- Inherit from `ValueObject` for complex scenarios requiring custom behavior
- Always validate input in constructors
- Make properties immutable

### 3. Entity Design

- Keep entities focused on their core business logic
- Use domain events for cross-aggregate communication
- Implement `IAggregateRoot` only on true aggregate roots
- Prefer composition over inheritance

### 4. Repository Usage

- Use specifications for complex queries
- Keep repositories focused on aggregate roots
- Implement custom repository interfaces for domain-specific operations
- Use `IReadRepository<T>` for read-only scenarios

## 🧪 Testing

The package includes comprehensive unit tests demonstrating proper usage patterns:

```bash
# Run tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📖 Examples and Templates

For complete implementation examples, see:

- **[Clean Architecture Template](https://github.com/ardalis/cleanarchitecture)** - Complete solution template
- **[NimblePros.SharedKernel](https://github.com/NimblePros/SharedKernelSample)** - Production example

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## 📄 License

This project is licensed under the [MIT License](LICENSE).

## 🙏 Acknowledgments

Created by [Steve Smith (@ardalis)](https://github.com/ardalis) as part of the Clean Architecture ecosystem.

Special thanks to the community contributors and the teams behind MediatR, Specification pattern, and other foundational libraries.

---

## 🔗 Related Projects

- [Clean Architecture Solution Template](https://github.com/ardalis/cleanarchitecture)
- [Ardalis.Specification](https://github.com/ardalis/Specification)
- [Ardalis.GuardClauses](https://github.com/ardalis/GuardClauses)
- [Ardalis.Result](https://github.com/ardalis/Result)

---

**💡 Pro Tip:** Star this repository to stay updated with the latest DDD and Clean Architecture patterns!
