using MediatR;

namespace WebSpark.SharedKernel.Behaviors;

/// <summary>
/// Represents a command in the CQRS pattern that returns a response.
/// Commands are used for operations that modify state and can return a result.
/// Source: https://code-maze.com/cqrs-mediatr-fluentvalidation/
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the command.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
