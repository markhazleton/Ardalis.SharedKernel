using MediatR;

namespace WebSpark.SharedKernel.Behaviors;

/// <summary>
/// Represents a handler for queries in the CQRS pattern.
/// Query handlers contain the logic for retrieving data without modifying state.
/// Source: https://code-maze.com/cqrs-mediatr-fluentvalidation/
/// </summary>
/// <typeparam name="TQuery">The type of query to handle.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the query.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
       where TQuery : IQuery<TResponse>
{
}
