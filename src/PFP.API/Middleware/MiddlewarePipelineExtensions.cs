namespace PFP.API.Middleware;

/// <summary>Registers custom API middleware in the correct order with exception safety.</summary>
public static class MiddlewarePipelineExtensions
{
    /// <summary>
    /// Adds <see cref="ExceptionHandlingMiddleware"/> (outermost) then <see cref="LocalizationMiddleware"/>.
    /// Exception handling must wrap all other custom middleware so failures never leave the pipeline unhandled.
    /// </summary>
    public static IApplicationBuilder UsePfpMiddleware(this IApplicationBuilder app)
    {
        return app
            .UseMiddleware<ExceptionHandlingMiddleware>()
            .UseMiddleware<LocalizationMiddleware>();
    }
}
