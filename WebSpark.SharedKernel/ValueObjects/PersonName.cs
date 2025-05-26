namespace WebSpark.SharedKernel.ValueObjects;

/// <summary>
/// Represents a person's name as a value object with validation and formatting capabilities.
/// Provides various representations of a person's name including full name, reverse name, and initials.
/// </summary>
public sealed class PersonName : ValueObject
{    /// <summary>
     /// Gets the first name of the person.
     /// </summary>
    public string FirstName { get; init; }

    /// <summary>
    /// Gets the last name of the person.
    /// </summary>
    public string LastName { get; init; }

    /// <summary>
    /// Private parameterless constructor for Entity Framework.
    /// </summary>
    private PersonName()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the PersonName class.
    /// </summary>
    /// <param name="first">The first name. Cannot be null or whitespace.</param>
    /// <param name="last">The last name. Cannot be null or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when first or last name is null or whitespace.</exception>
    public PersonName(string first, string last)
    {
        if (string.IsNullOrWhiteSpace(first))
            throw new ArgumentException("First name cannot be null or whitespace", nameof(first));
        if (string.IsNullOrWhiteSpace(last))
            throw new ArgumentException("Last name cannot be null or whitespace", nameof(last));

        FirstName = first.Trim();
        LastName = last.Trim();
    }

    /// <summary>
    /// Gets the full name in "FirstName LastName" format.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Gets the name in "LastName, FirstName" format.
    /// </summary>
    public string ReverseName => $"{LastName}, {FirstName}";

    /// <summary>
    /// Gets simple initials from the first letter of first and last names.
    /// </summary>
    public string SingleInitials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}";

    /// <summary>
    /// Gets complex initials using the first three characters of first and last names.
    /// Pads with underscores if the name is shorter than 3 characters.
    /// </summary>
    public string ComplexInitials =>
        $"{string.Concat(FirstName, "__").Substring(0, 3)}" +
        $"{string.Concat(LastName, "__").Substring(0, 3)}";

    /// <summary>
    /// Returns a string representation of the person's full name.
    /// </summary>
    /// <returns>The full name of the person.</returns>
    public override string ToString() => FullName;

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }
}
