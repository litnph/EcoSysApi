using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Hosting;
using PFP.Application.Common.Exceptions;

namespace PFP.API.Middleware;

/// <summary>Maps application-layer exceptions to HTTP responses (spec §6.3).</summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
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
        catch (ValidationException ex)
        {
            await WriteJsonAsync(
                    context,
                    HttpStatusCode.BadRequest,
                    "validation_error",
                    ex.Errors.Select(e => e.ErrorMessage).ToArray())
                .ConfigureAwait(false);
        }
        catch (NotFoundException ex)
        {
            await WriteJsonAsync(context, HttpStatusCode.NotFound, "not_found", new[] { ex.Message }).ConfigureAwait(false);
        }
        catch (BusinessRuleException ex)
        {
            await WriteJsonAsync(
                    context,
                    (HttpStatusCode)422,
                    "business_rule",
                    new[] { ex.Message })
                .ConfigureAwait(false);
        }
        catch (UnauthorizedAppException ex)
        {
            await WriteJsonAsync(
                    context,
                    HttpStatusCode.Unauthorized,
                    "unauthorized",
                    new[] { ex.Message })
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.");
            var publicMessage = "An unexpected error occurred.";
            if (_environment.IsDevelopment())
            {
                var inner = ex.InnerException is { } i ? $" | Inner: {i.GetType().Name}: {i.Message}" : string.Empty;
                publicMessage = $"{ex.GetType().Name}: {ex.Message}{inner}";
            }

            await WriteJsonAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "internal_error",
                    new[] { publicMessage })
                .ConfigureAwait(false);
        }
    }

    private static async Task WriteJsonAsync(
        HttpContext context,
        HttpStatusCode status,
        string code,
        string[] messages)
    {
        if (context.Response.HasStarted)
            return;

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
}
