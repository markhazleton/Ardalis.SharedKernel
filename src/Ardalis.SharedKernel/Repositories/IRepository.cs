using Ardalis.SharedKernel.Entities;
using Ardalis.Specification;

namespace Ardalis.SharedKernel.Repositories;

/// <summary>
/// An abstraction for persistence, based on Ardalis.Specification
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot
{
}
