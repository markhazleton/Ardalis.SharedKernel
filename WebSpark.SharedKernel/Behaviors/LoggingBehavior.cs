using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace WebSpark.SharedKernel.Behaviors;

/// <summary>
/// MediatR pipeline behavior that adds comprehensive logging for all requests.
/// Logs request details, property values, execution time, and responses.
/// Configure by adding the service with a scoped lifetime in your DI container.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response being returned.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
{
    private readonly ILogger<Mediator> _logger;

    /// <summary>
    /// Initializes a new instance of the LoggingBehavior class.
    /// </summary>
    /// <param name="logger">The logger instance for recording request processing information.</param>
    public LoggingBehavior(ILogger<Mediator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the request with comprehensive logging including property inspection and timing.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The response from the next handler in the pipeline.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);

            // Reflection! Could be a performance concern
            Type myType = request.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
            foreach (PropertyInfo prop in props)
            {
                object? propValue = prop?.GetValue(request, null);
                _logger.LogInformation("Property {Property} : {@Value}", prop?.Name, propValue);
            }
        }

        var sw = Stopwatch.StartNew();

        // Pass the cancellationToken and use ConfigureAwait(false) to address the diagnostics
        var response = await next(ct).ConfigureAwait(false);

        _logger.LogInformation("Handled {RequestName} with {Response} in {ms} ms", typeof(TRequest).Name, response, sw.ElapsedMilliseconds);
        sw.Stop();
        return response;
    }
}

