using WebSpark.SharedKernel.Entities;
using WebSpark.SharedKernel.ValueObjects;
using WebSpark.SharedKernel.DomainEvents;

namespace WebSpark.SharedKernel.Examples;

/// <summary>
/// Example aggregate root entity demonstrating proper domain modeling with value objects and domain events.
/// </summary>
public class Customer : EntityBase, IAggregateRoot
{
    public PersonName Name { get; private set; }
    public Email Email { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private readonly List<Address> _addresses = new();
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

    // Required for EF Core
    private Customer() : base() { }

    public Customer(PersonName name, Email email) : base()
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Status = CustomerStatus.Active;
        CreatedAt = DateTime.UtcNow;

        RegisterDomainEvent(new CustomerCreatedEvent(Id, name.FullName, email.Value));
    }

    public void UpdateEmail(Email newEmail)
    {
        if (newEmail == null) throw new ArgumentNullException(nameof(newEmail));

        if (!Email.Equals(newEmail))
        {
            var oldEmail = Email;
            Email = newEmail;
            RegisterDomainEvent(new CustomerEmailChangedEvent(Id, oldEmail.Value, newEmail.Value));
        }
    }

    public void UpdateName(PersonName newName)
    {
        if (newName == null) throw new ArgumentNullException(nameof(newName));

        if (!Name.Equals(newName))
        {
            var oldName = Name;
            Name = newName;
            RegisterDomainEvent(new CustomerNameChangedEvent(Id, oldName.FullName, newName.FullName));
        }
    }

    public void AddAddress(Address address)
    {
        if (address == null) throw new ArgumentNullException(nameof(address));

        _addresses.Add(address);
        RegisterDomainEvent(new CustomerAddressAddedEvent(Id, address));
    }

    public void Deactivate()
    {
        if (Status == CustomerStatus.Active)
        {
            Status = CustomerStatus.Inactive;
            RegisterDomainEvent(new CustomerDeactivatedEvent(Id));
        }
    }

    public void Reactivate()
    {
        if (Status == CustomerStatus.Inactive)
        {
            Status = CustomerStatus.Active;
            RegisterDomainEvent(new CustomerReactivatedEvent(Id));
        }
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        RegisterDomainEvent(new CustomerLoginRecordedEvent(Id, LastLoginAt.Value));
    }
}

public enum CustomerStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3
}

/// <summary>
/// Email value object with validation
/// </summary>
public class Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));

        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format", nameof(value));

        Value = value.ToLowerInvariant().Trim();
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

    public override string ToString() => Value;
}

/// <summary>
/// Address value object
/// </summary>
public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public Address(string street, string city, string state, string postalCode, string country)
    {
        Street = !string.IsNullOrWhiteSpace(street) ? street.Trim() : throw new ArgumentException("Street cannot be empty", nameof(street));
        City = !string.IsNullOrWhiteSpace(city) ? city.Trim() : throw new ArgumentException("City cannot be empty", nameof(city));
        State = !string.IsNullOrWhiteSpace(state) ? state.Trim() : throw new ArgumentException("State cannot be empty", nameof(state));
        PostalCode = !string.IsNullOrWhiteSpace(postalCode) ? postalCode.Trim() : throw new ArgumentException("Postal code cannot be empty", nameof(postalCode));
        Country = !string.IsNullOrWhiteSpace(country) ? country.Trim() : throw new ArgumentException("Country cannot be empty", nameof(country));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString() => $"{Street}, {City}, {State} {PostalCode}, {Country}";
}

// Domain Events
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

public class CustomerNameChangedEvent : DomainEventBase
{
    public int CustomerId { get; }
    public string OldName { get; }
    public string NewName { get; }

    public CustomerNameChangedEvent(int customerId, string oldName, string newName)
    {
        CustomerId = customerId;
        OldName = oldName;
        NewName = newName;
    }
}

public class CustomerAddressAddedEvent : DomainEventBase
{
    public int CustomerId { get; }
    public Address Address { get; }

    public CustomerAddressAddedEvent(int customerId, Address address)
    {
        CustomerId = customerId;
        Address = address;
    }
}

public class CustomerDeactivatedEvent : DomainEventBase
{
    public int CustomerId { get; }

    public CustomerDeactivatedEvent(int customerId)
    {
        CustomerId = customerId;
    }
}

public class CustomerReactivatedEvent : DomainEventBase
{
    public int CustomerId { get; }

    public CustomerReactivatedEvent(int customerId)
    {
        CustomerId = customerId;
    }
}

public class CustomerLoginRecordedEvent : DomainEventBase
{
    public int CustomerId { get; }
    public DateTime LoginTime { get; }

    public CustomerLoginRecordedEvent(int customerId, DateTime loginTime)
    {
        CustomerId = customerId;
        LoginTime = loginTime;
    }
}
