using Microsoft.Extensions.Hosting;

namespace PFP.API.Middleware;

/// <summary>Maps application-layer exceptions to HTTP responses (spec §6.3).</summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>Creates the middleware.</summary>
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>Invokes the next delegate and translates failures into JSON problem payloads.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await ApiExceptionResponseWriter
                .TryWriteAsync(context, ex, _environment, _logger)
                .ConfigureAwait(false);
        }
    }
}
