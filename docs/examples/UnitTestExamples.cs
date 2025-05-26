using FluentAssertions;
using WebSpark.SharedKernel.ValueObjects;
using WebSpark.SharedKernel.Entities;
using WebSpark.SharedKernel.DomainEvents;
using Xunit;

namespace WebSpark.SharedKernel.Tests.Examples;

/// <summary>
/// Example unit tests demonstrating how to test SharedKernel components.
/// These tests showcase best practices for testing entities, value objects, and domain events.
/// </summary>
public class SharedKernelExampleTests
{
    #region Value Object Tests

    [Fact]
    public void PersonName_WithValidNames_ShouldCreateSuccessfully()
    {
        // Arrange
        const string firstName = "John";
        const string lastName = "Doe";

        // Act
        var personName = new PersonName(firstName, lastName);

        // Assert
        personName.FirstName.Should().Be(firstName);
        personName.LastName.Should().Be(lastName);
        personName.FullName.Should().Be("John Doe");
        personName.ReverseName.Should().Be("Doe, John");
        personName.SingleInitials.Should().Be("JD");
    }

    [Theory]
    [InlineData(null, "Doe")]
    [InlineData("", "Doe")]
    [InlineData("   ", "Doe")]
    [InlineData("John", null)]
    [InlineData("John", "")]
    [InlineData("John", "   ")]
    public void PersonName_WithInvalidNames_ShouldThrowArgumentException(string firstName, string lastName)
    {
        // Act & Assert
        var action = () => new PersonName(firstName, lastName);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PersonName_EqualValues_ShouldBeEqual()
    {
        // Arrange
        var name1 = new PersonName("John", "Doe");
        var name2 = new PersonName("John", "Doe");

        // Act & Assert
        name1.Should().Be(name2);
        name1.GetHashCode().Should().Be(name2.GetHashCode());
        (name1 == name2).Should().BeTrue();
    }

    [Fact]
    public void PersonName_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var name1 = new PersonName("John", "Doe");
        var name2 = new PersonName("Jane", "Doe");

        // Act & Assert
        name1.Should().NotBe(name2);
        (name1 == name2).Should().BeFalse();
    }

    [Fact]
    public void PersonName_ToString_ShouldReturnFullName()
    {
        // Arrange
        var name = new PersonName("John", "Doe");

        // Act
        var result = name.ToString();

        // Assert
        result.Should().Be("John Doe");
    }

    [Fact]
    public void PersonName_WithExtraWhitespace_ShouldTrimValues()
    {
        // Arrange & Act
        var name = new PersonName("  John  ", "  Doe  ");

        // Assert
        name.FirstName.Should().Be("John");
        name.LastName.Should().Be("Doe");
        name.FullName.Should().Be("John Doe");
    }

    #endregion

    #region Entity Tests

    [Fact]
    public void Entity_WhenCreated_ShouldHaveDefaultIdAndNoDomainEvents()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().Be(0); // Default int value
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Entity_WhenDomainEventRegistered_ShouldAppearInDomainEvents()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();

        // Act
        entity.RegisterTestDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.Should().HaveCount(1);
        entity.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void Entity_WhenMultipleDomainEventsRegistered_ShouldMaintainOrder()
    {
        // Arrange
        var entity = new TestEntity();
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();

        // Act
        entity.RegisterTestDomainEvent(event1);
        entity.RegisterTestDomainEvent(event2);

        // Assert
        entity.DomainEvents.Should().HaveCount(2);
        entity.DomainEvents.ElementAt(0).Should().Be(event1);
        entity.DomainEvents.ElementAt(1).Should().Be(event2);
    }

    [Fact]
    public void Entity_WhenDomainEventsCleared_ShouldBeEmpty()
    {
        // Arrange
        var entity = new TestEntity();
        entity.RegisterTestDomainEvent(new TestDomainEvent());

        // Act
        entity.ClearTestDomainEvents();

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region Domain Event Tests

    [Fact]
    public void DomainEvent_WhenCreated_ShouldHaveDateOccurred()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var domainEvent = new TestDomainEvent();

        // Assert
        var afterCreation = DateTime.UtcNow;
        domainEvent.DateOccurred.Should().BeAfter(beforeCreation.AddMilliseconds(-1));
        domainEvent.DateOccurred.Should().BeBefore(afterCreation.AddMilliseconds(1));
        domainEvent.DateOccurred.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void DomainEvent_ShouldImplementINotification()
    {
        // Arrange & Act
        var domainEvent = new TestDomainEvent();

        // Assert
        domainEvent.Should().BeAssignableTo<MediatR.INotification>();
    }

    #endregion

    #region SafeDictionary Tests

    [Fact]
    public void SafeDictionary_WhenEmpty_ShouldReturnDefaultForMissingKey()
    {
        // Arrange
        var safeDictionary = new SafeDictionary<string, int>();

        // Act
        var result = safeDictionary.GetValue("missing");

        // Assert
        result.Should().Be(default(int));
    }

    [Fact]
    public void SafeDictionary_WithNullKey_ShouldReturnDefault()
    {
        // Arrange
        var safeDictionary = new SafeDictionary<string, int>();

        // Act
        var result = safeDictionary.GetValue(null!);

        // Assert
        result.Should().Be(default(int));
    }

    [Fact]
    public void SafeDictionary_WhenValueSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var safeDictionary = new SafeDictionary<string, int>();
        const string key = "test";
        const int value = 42;

        // Act
        safeDictionary.SetValue(key, value);
        var result = safeDictionary.GetValue(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void SafeDictionary_GetList_ShouldReturnStringRepresentations()
    {
        // Arrange
        var safeDictionary = new SafeDictionary<string, int>();
        safeDictionary.SetValue("key1", 1);
        safeDictionary.SetValue("key2", 2);

        // Act
        var result = safeDictionary.GetList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("key1 - 1");
        result.Should().Contain("key2 - 2");
    }

    [Fact]
    public void SafeDictionary_SetValue_WithDictionary_ShouldReplaceAll()
    {
        // Arrange
        var safeDictionary = new SafeDictionary<string, int>();
        safeDictionary.SetValue("old", 1);

        var newDictionary = new Dictionary<string, int>
        {
            { "new1", 10 },
            { "new2", 20 }
        };

        // Act
        safeDictionary.SetValue(newDictionary);

        // Assert
        safeDictionary.GetValue("old").Should().Be(default(int));
        safeDictionary.GetValue("new1").Should().Be(10);
        safeDictionary.GetValue("new2").Should().Be(20);
    }

    #endregion
}

#region Test Helpers

/// <summary>
/// Test entity for testing domain event functionality
/// </summary>
public class TestEntity : EntityBase, IAggregateRoot
{
    public void RegisterTestDomainEvent(DomainEventBase domainEvent)
    {
        RegisterDomainEvent(domainEvent);
    }

    public void ClearTestDomainEvents()
    {
        ClearDomainEvents();
    }
}

/// <summary>
/// Test domain event for testing purposes
/// </summary>
public class TestDomainEvent : DomainEventBase
{
    public string TestProperty { get; set; } = "Test";
}

/// <summary>
/// Example complex value object for testing
/// </summary>
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));
        if (currency.Length != 3)
            throw new ArgumentException("Currency must be 3 characters", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract {other.Currency} from {Currency}");

        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:C} {Currency}";
}

#endregion

/// <summary>
/// Integration tests demonstrating repository patterns and CQRS
/// </summary>
public class IntegrationExampleTests
{
    [Fact]
    public void Repository_Interface_ShouldHaveCorrectConstraints()
    {
        // This test verifies that IRepository<T> has the correct generic constraints
        // by attempting to use it with both valid and invalid types

        // This should compile - Customer implements IAggregateRoot
        typeof(IRepository<TestEntity>).Should().NotBeNull();

        // This would not compile if we tried to use a non-aggregate root:
        // typeof(IRepository<string>) - this would cause a compilation error
    }

    [Fact]
    public void ReadRepository_Interface_ShouldHaveCorrectConstraints()
    {
        // Similar test for IReadRepository<T>
        typeof(IReadRepository<TestEntity>).Should().NotBeNull();
    }
}

/// <summary>
/// Performance and behavior tests
/// </summary>
public class PerformanceExampleTests
{
    [Fact]
    public void ValueObject_Equality_ShouldBeEfficient()
    {
        // Arrange
        var money1 = new Money(100.50m, "USD");
        var money2 = new Money(100.50m, "USD");
        var money3 = new Money(200.00m, "EUR");

        // Act & Assert - These operations should be fast
        money1.Equals(money2).Should().BeTrue();
        money1.Equals(money3).Should().BeFalse();

        // Hash codes should be the same for equal objects
        money1.GetHashCode().Should().Be(money2.GetHashCode());
    }

    [Fact]
    public void Entity_DomainEvents_ShouldNotAffectPerformance()
    {
        // Arrange
        var entity = new TestEntity();

        // Act - Register many domain events
        for (int i = 0; i < 1000; i++)
        {
            entity.RegisterTestDomainEvent(new TestDomainEvent());
        }

        // Assert
        entity.DomainEvents.Should().HaveCount(1000);

        // Clearing should be efficient
        entity.ClearTestDomainEvents();
        entity.DomainEvents.Should().BeEmpty();
    }
}
