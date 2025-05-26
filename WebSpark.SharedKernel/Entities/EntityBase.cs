namespace WebSpark.SharedKernel.Entities;

/// <summary>
/// A base class for DDD Entities. Includes support for domain events dispatched post-persistence.
/// If you prefer GUID Ids, change it here.
/// If you need to support both GUID and int IDs, change to EntityBase&lt;TId&gt; and use TId as the type for Id.
/// </summary>
public abstract class EntityBase : HasDomainEventsBase
{
    /// <summary>
    /// Gets or sets the unique identifier for this entity.
    /// </summary>
    public int Id { get; set; }
}

/// <summary>
/// A base class for DDD Entities with strongly-typed IDs. Includes support for domain events dispatched post-persistence.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier. Must be a struct that implements IEquatable.</typeparam>
public abstract class EntityBase<TId> : HasDomainEventsBase
  where TId : struct, IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the unique identifier for this entity.
    /// </summary>
    public TId Id { get; set; } = default!;
}

/// <summary>
/// For use with Vogen or similar tools for generating code for 
/// strongly typed Ids. Provides entity base functionality with custom ID types.
/// </summary>
/// <typeparam name="T">The entity type (for self-referencing constraint).</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class EntityBase<T, TId> : HasDomainEventsBase
  where T : EntityBase<T, TId>
{
    /// <summary>
    /// Gets or sets the unique identifier for this entity.
    /// </summary>
    public TId Id { get; set; } = default!;
}

