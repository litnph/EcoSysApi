using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline stage that records the lifecycle of every request (spec §2.3).
/// <para>
/// Logs request start, structured request name, elapsed time, and any thrown exception with
/// the caller's <c>UserId</c> / <c>SessionId</c> when available. Sensitive payload data
/// (passwords, tokens, card numbers) is not serialised — only the request type name is logged.
/// </para>
/// <para>
/// Runs as the outermost behaviour so that downstream validation / authorisation / handler
/// timings are all captured inside its elapsed measurement.
/// </para>
/// </summary>
/// <typeparam name="TRequest">Incoming command or query.</typeparam>
/// <typeparam name="TResponse">Handler return type.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the behaviour.</summary>
    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUser.UserId;
        var sessionId = _currentUser.SessionId;

        _logger.LogInformation(
            "MediatR request {RequestName} started for user={UserId} session={SessionId}.",
            requestName,
            userId,
            sessionId);

        var sw = Stopwatch.StartNew();

        try
        {
            var response = await next().ConfigureAwait(false);
            sw.Stop();

            _logger.LogInformation(
                "MediatR request {RequestName} completed in {ElapsedMs}ms.",
                requestName,
                sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning(
                ex,
                "MediatR request {RequestName} failed after {ElapsedMs}ms: {ExceptionType}.",
                requestName,
                sw.ElapsedMilliseconds,
                ex.GetType().Name);
            throw;
        }
    }
}
