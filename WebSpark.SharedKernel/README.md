# WebSpark.SharedKernel

[![NuGet](https://img.shields.io/nuget/v/WebSpark.SharedKernel.svg)](https://www.nuget.org/packages/WebSpark.SharedKernel/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/WebSpark.SharedKernel.svg)](https://www.nuget.org/packages/WebSpark.SharedKernel/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive collection of base classes and utilities for implementing Domain-Driven Design (DDD) patterns in .NET applications. This package provides essential building blocks for clean architecture implementations, including entities, value objects, domain events, and repository abstractions.

## 🎯 Why WebSpark.SharedKernel?

WebSpark.SharedKernel simplifies the implementation of common DDD patterns by providing:

- **Type-safe entity base classes** with support for multiple ID types
- **Robust value object implementation** with proper equality semantics
- **Domain event infrastructure** with MediatR integration
- **Repository pattern abstractions** built on WebSpark.Specification
- **CQRS pattern support** with command and query abstractions
- **Comprehensive logging** for all MediatR requests

## 📦 Installation

```bash
dotnet add package WebSpark.SharedKernel
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

See the [full documentation](https://github.com/MarkHazleton/WebSpark.SharedKernel) for details on all entities, value objects, domain events, repository interfaces, and CQRS support.

## 🔧 Dependencies

- [MediatR](https://www.nuget.org/packages/MediatR)
- [WebSpark.Specification](https://github.com/ardalis/Specification)
- [WebSpark.GuardClauses](https://github.com/ardalis/GuardClauses)
- [Microsoft.Extensions.Logging.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions)

## 📋 Requirements

- .NET 8.0 or later
- C# 10+

## 🏆 Best Practices

- Create your own SharedKernel for production use
- Use `readonly record struct` for simple value objects
- Keep entities focused on business logic
- Use domain events for cross-aggregate communication
- Use specifications for complex queries
- Test domain logic in isolation

## 🧪 Testing

```bash
dotnet test
```

## 📄 License

This project is licensed under the [MIT License](LICENSE).

## 🤝 Contributing

Contributions are welcome! Please see the repository for guidelines.

---

**Pro Tip:** Star this repository to stay updated with the latest DDD and Clean Architecture patterns!
