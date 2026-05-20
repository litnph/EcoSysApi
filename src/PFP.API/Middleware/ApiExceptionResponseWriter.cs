using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Hosting;
using PFP.Application.Common.Exceptions;
using PFP.Domain.Exceptions;

namespace PFP.API.Middleware;

/// <summary>Maps exceptions to the standard API error JSON envelope (spec §6.3).</summary>
internal static class ApiExceptionResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Writes a problem JSON payload when the response has not started; otherwise aborts the connection.</summary>
    public static async Task TryWriteAsync(
        HttpContext context,
        Exception exception,
        IHostEnvironment environment,
        ILogger logger)
    {
        if (context.Response.HasStarted)
        {
            logger.LogWarning(
                exception,
                "Exception after response started for {Method} {Path}; aborting connection.",
                context.Request.Method,
                context.Request.Path);
            context.Abort();
            return;
        }

        var (status, code, messages) = MapException(exception, environment);

        if (status == HttpStatusCode.InternalServerError)
            logger.LogError(exception, "Unhandled exception for {Method} {Path}.", context.Request.Method, context.Request.Path);

        try
        {
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json; charset=utf-8";

            var body = new
            {
                success = false,
                data = (object?)null,
                error = new { code, messages },
                meta = (object?)null,
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions)).ConfigureAwait(false);
        }
        catch (Exception writeEx)
        {
            logger.LogError(
                writeEx,
                "Failed to write error response for {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);
            context.Abort();
        }
    }

    private static (HttpStatusCode Status, string Code, string[] Messages) MapException(
        Exception exception,
        IHostEnvironment environment)
    {
        return exception switch
        {
            ValidationException ex => (
                HttpStatusCode.BadRequest,
                "validation_error",
                ex.Errors.Select(e => e.ErrorMessage).ToArray()),
            NotFoundException ex => (HttpStatusCode.NotFound, "not_found", new[] { ex.Message }),
            ForbiddenException ex => (HttpStatusCode.Forbidden, "forbidden", new[] { ex.Message }),
            BusinessRuleException ex => ((HttpStatusCode)422, "business_rule", new[] { ex.Message }),
            DomainException ex => ((HttpStatusCode)422, "domain_rule", new[] { ex.Message }),
            UnauthorizedAppException ex => (HttpStatusCode.Unauthorized, "unauthorized", new[] { ex.Message }),
            OperationCanceledException => (
                (HttpStatusCode)499,
                "request_cancelled",
                new[] { "The request was cancelled." }),
            _ => (
                HttpStatusCode.InternalServerError,
                "internal_error",
                new[] { BuildInternalErrorMessage(exception, environment) }),
        };
    }

    private static string BuildInternalErrorMessage(Exception exception, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
            return "An unexpected error occurred.";

        var inner = exception.InnerException is { } i
            ? $" | Inner: {i.GetType().Name}: {i.Message}"
            : string.Empty;

        return $"{exception.GetType().Name}: {exception.Message}{inner}";
    }
}
