# Getting Started with WebSpark.SharedKernel

This guide will help you get up and running with WebSpark.SharedKernel in your .NET application.

## Installation

### Package Manager Console

```powershell
Install-Package WebSpark.SharedKernel
```

### .NET CLI

```bash
dotnet add package WebSpark.SharedKernel
```

### PackageReference

```xml
<PackageReference Include="WebSpark.SharedKernel" Version="1.0.0" />
```

## Quick Start

### 1. Define Your First Entity

```csharp
using WebSpark.SharedKernel.Entities;
using WebSpark.SharedKernel.ValueObjects;

public class Customer : EntityBase, IAggregateRoot
{
    public PersonName Name { get; private set; }
    public string Email { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Customer() { } // EF Core constructor

    public Customer(PersonName name, string email)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        CreatedAt = DateTime.UtcNow;

        // Raise a domain event
        RegisterDomainEvent(new CustomerCreatedEvent(Id, name.FullName, email));
    }

    public void UpdateEmail(string newEmail)
    {
        if (Email != newEmail)
        {
            var oldEmail = Email;
            Email = newEmail;
            RegisterDomainEvent(new CustomerEmailChangedEvent(Id, oldEmail, newEmail));
        }
    }
}
```

### 2. Create Value Objects

```csharp
using WebSpark.SharedKernel.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));
        
        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format", nameof(value));
            
        Value = value.ToLowerInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string email) => new(email);
}
```

### 3. Define Domain Events

```csharp
using WebSpark.SharedKernel.DomainEvents;

public class CustomerCreatedEvent : DomainEventBase
{
    public int CustomerId { get; }
    public string CustomerName { get; }
    public string Email { get; }

    public CustomerCreatedEvent(int customerId, string customerName, string email)
    {
        CustomerId = customerId;
        CustomerName = customerName;
        Email = email;
    }
}

public class CustomerEmailChangedEvent : DomainEventBase
{
    public int CustomerId { get; }
    public string OldEmail { get; }
    public string NewEmail { get; }

    public CustomerEmailChangedEvent(int customerId, string oldEmail, string newEmail)
    {
        CustomerId = customerId;
        OldEmail = oldEmail;
        NewEmail = newEmail;
    }
}
```

### 4. Create Event Handlers

```csharp
using MediatR;
using Microsoft.Extensions.Logging;

public class CustomerCreatedEventHandler : INotificationHandler<CustomerCreatedEvent>
{
    private readonly ILogger<CustomerCreatedEventHandler> _logger;

    public CustomerCreatedEventHandler(ILogger<CustomerCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CustomerCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Customer created: {CustomerId} - {CustomerName} ({Email})",
            notification.CustomerId,
            notification.CustomerName,
            notification.Email);

        // Add your business logic here (e.g., send welcome email, create audit log, etc.)
        
        return Task.CompletedTask;
    }
}
```

### 5. Set Up Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using WebSpark.SharedKernel.DomainEvents;
using WebSpark.SharedKernel.Behaviors;

public void ConfigureServices(IServiceCollection services)
{
    // Add MediatR
    services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        cfg.AddBehavior<LoggingBehavior<,>>();
    });

    // Add domain event dispatcher
    services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

    // Add your repositories, services, etc.
    services.AddScoped<ICustomerRepository, CustomerRepository>();
}
```

### 6. Create Repository Interface

```csharp
using WebSpark.SharedKernel.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
```

## CQRS Pattern Support

### Commands and Queries

```csharp
using WebSpark.SharedKernel.Behaviors;

// Command
public record CreateCustomerCommand(string FirstName, string LastName, string Email) 
    : ICommand<int>;

// Command Handler
public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, int>
{
    private readonly ICustomerRepository _repository;

    public CreateCustomerCommandHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var name = new PersonName(request.FirstName, request.LastName);
        var customer = new Customer(name, request.Email);
        
        await _repository.AddAsync(customer, cancellationToken);
        
        return customer.Id;
    }
}

// Query
public record GetCustomerQuery(int CustomerId) : IQuery<CustomerDto?>;

// Query Handler
public class GetCustomerQueryHandler : IQueryHandler<GetCustomerQuery, CustomerDto?>
{
    private readonly IReadRepository<Customer> _repository;

    public GetCustomerQueryHandler(IReadRepository<Customer> repository)
    {
        _repository = repository;
    }

    public async Task<CustomerDto?> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.CustomerId, cancellationToken);
        
        return customer != null ? new CustomerDto(
            customer.Id,
            customer.Name.FullName,
            customer.Email,
            customer.CreatedAt) : null;
    }
}
```

## Entity Framework Core Integration

```csharp
using Microsoft.EntityFrameworkCore;
using WebSpark.SharedKernel.DomainEvents;

public class ApplicationDbContext : DbContext
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDomainEventDispatcher domainEventDispatcher) : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    public DbSet<Customer> Customers { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToArray();

        var result = await base.SaveChangesAsync(cancellationToken);
        
        await _domainEventDispatcher.DispatchAndClearEvents(entitiesWithEvents);
        
        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure PersonName as owned entity
        modelBuilder.Entity<Customer>()
            .OwnsOne(c => c.Name, name =>
            {
                name.Property(n => n.FirstName).HasMaxLength(100);
                name.Property(n => n.LastName).HasMaxLength(100);
            });

        base.OnModelCreating(modelBuilder);
    }
}
```

## Next Steps

1. Explore the [API Documentation](api-reference.md)
2. Check out more [Examples](examples/)
3. Learn about [Best Practices](best-practices.md)
4. Read about [Advanced Scenarios](advanced-scenarios.md)
