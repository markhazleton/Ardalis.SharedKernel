using MediatR;

namespace WebSpark.SharedKernel.Behaviors;

/// <summary>
/// Represents a query in the CQRS pattern that returns data without modifying state.
/// Queries are used for read operations and should be side-effect free.
/// Source: https://code-maze.com/cqrs-mediatr-fluentvalidation/
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the query.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
