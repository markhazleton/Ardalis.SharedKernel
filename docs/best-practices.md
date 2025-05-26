# Best Practices for WebSpark.SharedKernel

This document outlines recommended practices when using WebSpark.SharedKernel in your applications.

## Entity Design

### ✅ DO: Keep Entities Focused

```csharp
// Good: Focused entity with clear responsibilities
public class Order : EntityBase, IAggregateRoot
{
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; }
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public void AddItem(Product product, int quantity)
    {
        // Business logic here
        RegisterDomainEvent(new OrderItemAddedEvent(Id, product.Id, quantity));
    }
}
```

### ❌ DON'T: Create God Entities

```csharp
// Bad: Too many responsibilities
public class Customer : EntityBase, IAggregateRoot
{
    // Customer data
    public string Name { get; set; }
    public string Email { get; set; }
    
    // Order data (belongs in separate aggregate)
    public List<Order> Orders { get; set; }
    
    // Payment data (belongs in separate aggregate)
    public List<PaymentMethod> PaymentMethods { get; set; }
    
    // Shipping data (belongs in separate aggregate)
    public List<ShippingAddress> Addresses { get; set; }
}
```

## Value Object Guidelines

### ✅ DO: Validate in Constructor

```csharp
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
    
    // ...rest of implementation
}
```

### ✅ DO: Make Properties Immutable

```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
            
        return new Money(Amount + other.Amount, Currency);
    }
    
    // ...rest of implementation
}
```

### ✅ DO: Consider Using Records for Simple Value Objects

```csharp
// For C# 10+ and simple value objects
public readonly record struct ProductCode(string Value)
{
    public ProductCode(string value) : this(ValidateAndFormat(value)) { }
    
    private static string ValidateAndFormat(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Product code cannot be empty");
            
        return value.ToUpperInvariant();
    }
}
```

## Domain Events

### ✅ DO: Make Events Immutable

```csharp
public class OrderCreatedEvent : DomainEventBase
{
    public int OrderId { get; }
    public int CustomerId { get; }
    public decimal TotalAmount { get; }

    public OrderCreatedEvent(int orderId, int customerId, decimal totalAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }
}
```

### ✅ DO: Use Past Tense for Event Names

```csharp
// Good
public class CustomerCreatedEvent : DomainEventBase { }
public class OrderShippedEvent : DomainEventBase { }
public class PaymentProcessedEvent : DomainEventBase { }

// Bad
public class CreateCustomerEvent : DomainEventBase { }
public class ShipOrderEvent : DomainEventBase { }
public class ProcessPaymentEvent : DomainEventBase { }
```

### ✅ DO: Keep Events Focused

```csharp
// Good: Focused events
public class CustomerEmailChangedEvent : DomainEventBase
{
    public int CustomerId { get; }
    public string OldEmail { get; }
    public string NewEmail { get; }
    // ...
}

public class CustomerAddressChangedEvent : DomainEventBase
{
    public int CustomerId { get; }
    public Address OldAddress { get; }
    public Address NewAddress { get; }
    // ...
}

// Bad: Generic event that tries to handle everything
public class CustomerUpdatedEvent : DomainEventBase
{
    public int CustomerId { get; }
    public Dictionary<string, object> Changes { get; }
    // ...
}
```

## Repository Patterns

### ✅ DO: Use Specifications for Complex Queries

```csharp
public class ActiveCustomersSpec : Specification<Customer>
{
    public ActiveCustomersSpec()
    {
        Query.Where(c => c.IsActive)
             .OrderBy(c => c.Name.LastName)
             .ThenBy(c => c.Name.FirstName);
    }
}

public class CustomersByEmailDomainSpec : Specification<Customer>
{
    public CustomersByEmailDomainSpec(string domain)
    {
        Query.Where(c => c.Email.EndsWith($"@{domain}"));
    }
}

// Usage
var activeCustomers = await _repository.ListAsync(new ActiveCustomersSpec());
var companyCustomers = await _repository.ListAsync(new CustomersByEmailDomainSpec("company.com"));
```

### ✅ DO: Keep Repository Interfaces Focused on Aggregates

```csharp
// Good: Repository for aggregate root only
public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(CustomerId customerId);
}

// Bad: Repository that spans multiple aggregates
public interface IOrderRepository : IRepository<Order>
{
    Task<Customer> GetCustomerByOrderAsync(int orderId); // Should use CustomerRepository
    Task<Product> GetProductByIdAsync(int productId);   // Should use ProductRepository
}
```

## CQRS Patterns

### ✅ DO: Separate Commands and Queries

```csharp
// Commands modify state
public record CreateOrderCommand(int CustomerId, List<OrderItemDto> Items) : ICommand<int>;
public record CancelOrderCommand(int OrderId) : ICommand;

// Queries read state
public record GetOrderQuery(int OrderId) : IQuery<OrderDto?>;
public record GetOrdersByCustomerQuery(int CustomerId, int Page, int PageSize) : IQuery<PagedResult<OrderDto>>;
```

### ✅ DO: Use DTOs for Query Results

```csharp
public record OrderDto(
    int Id,
    string OrderNumber,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    DateTime CreatedDate,
    List<OrderItemDto> Items);

public record OrderItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
```

## Error Handling

### ✅ DO: Use Domain-Specific Exceptions

```csharp
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class InvalidEmailException : DomainException
{
    public InvalidEmailException(string email) 
        : base($"Invalid email format: {email}") { }
}

public class InsufficientInventoryException : DomainException
{
    public int ProductId { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    public InsufficientInventoryException(int productId, int requested, int available)
        : base($"Insufficient inventory for product {productId}. Requested: {requested}, Available: {available}")
    {
        ProductId = productId;
        RequestedQuantity = requested;
        AvailableQuantity = available;
    }
}
```

## Testing

### ✅ DO: Test Domain Logic in Isolation

```csharp
[Test]
public void Order_AddItem_ShouldRaiseDomainEvent()
{
    // Arrange
    var order = new Order(customerId: 1);
    var product = new Product("Test Product", 10.00m);
    
    // Act
    order.AddItem(product, quantity: 2);
    
    // Assert
    Assert.That(order.DomainEvents, Has.Count.EqualTo(1));
    Assert.That(order.DomainEvents.First(), Is.TypeOf<OrderItemAddedEvent>());
}

[Test]
public void Email_InvalidFormat_ShouldThrowException()
{
    // Act & Assert
    Assert.Throws<ArgumentException>(() => new Email("invalid-email"));
}
```

### ✅ DO: Test Value Object Equality

```csharp
[Test]
public void Money_EqualValues_ShouldBeEqual()
{
    // Arrange
    var money1 = new Money(100.00m, "USD");
    var money2 = new Money(100.00m, "USD");
    
    // Act & Assert
    Assert.That(money1, Is.EqualTo(money2));
    Assert.That(money1.GetHashCode(), Is.EqualTo(money2.GetHashCode()));
}
```

## Performance Considerations

### ✅ DO: Use Async/Await Properly

```csharp
public async Task<Customer> CreateCustomerAsync(string name, string email)
{
    var customer = new Customer(name, email);
    await _repository.AddAsync(customer);
    return customer;
}
```

### ✅ DO: Consider Using Specifications for Database Queries

```csharp
public class RecentOrdersSpec : Specification<Order>
{
    public RecentOrdersSpec(DateTime since)
    {
        Query.Where(o => o.CreatedDate >= since)
             .Include(o => o.Items)
             .ThenInclude(i => i.Product);
    }
}
```

### ❌ DON'T: Load Entire Aggregates for Simple Queries

```csharp
// Bad: Loading entire Order aggregate just to get customer name
public async Task<string> GetCustomerNameByOrderAsync(int orderId)
{
    var order = await _orderRepository.GetByIdAsync(orderId);
    return order.Customer.Name;
}

// Good: Use a specific query/specification
public async Task<string> GetCustomerNameByOrderAsync(int orderId)
{
    var spec = new CustomerNameByOrderSpec(orderId);
    return await _readRepository.FirstOrDefaultAsync(spec);
}
```

## Security

### ✅ DO: Validate Input in Value Objects

```csharp
public class SocialSecurityNumber : ValueObject
{
    public string Value { get; }

    public SocialSecurityNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SSN cannot be empty");
            
        // Remove formatting
        var cleaned = Regex.Replace(value, @"[^\d]", "");
        
        if (cleaned.Length != 9)
            throw new ArgumentException("SSN must be 9 digits");
            
        Value = cleaned;
    }
    
    public string ToFormattedString() => $"{Value.Substring(0, 3)}-{Value.Substring(3, 2)}-{Value.Substring(5, 4)}";
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### ✅ DO: Use Authorization in Command/Query Handlers

```csharp
public class DeleteOrderCommandHandler : ICommandHandler<DeleteOrderCommand>
{
    private readonly IOrderRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public async Task Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.OrderId);
        
        if (order == null)
            throw new NotFoundException($"Order {request.OrderId} not found");
            
        if (order.CustomerId != _currentUser.CustomerId && !_currentUser.IsAdmin)
            throw new UnauthorizedAccessException("Cannot delete another customer's order");
            
        await _repository.DeleteAsync(order);
    }
}
```
