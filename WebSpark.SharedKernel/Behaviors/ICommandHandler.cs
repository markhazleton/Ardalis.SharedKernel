using MediatR;

namespace WebSpark.SharedKernel.Behaviors;

/// <summary>
/// Represents a handler for commands in the CQRS pattern.
/// Command handlers contain the business logic for processing commands that modify state.
/// Source: https://code-maze.com/cqrs-mediatr-fluentvalidation/
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the command.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
{
}
