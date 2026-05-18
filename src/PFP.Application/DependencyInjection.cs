using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PFP.Application.Common.Behaviors;

namespace PFP.Application;

/// <summary>Registers Application-layer services (MediatR, FluentValidation, pipeline behaviours).</summary>
public static class DependencyInjection
{
    /// <summary>Adds MediatR handlers, validators, and cross-cutting pipeline behaviours from this assembly.</summary>
    /// <param name="services">Service collection.</param>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);

        // Pipeline order (spec §2.3): Request → Logging → Validation → Authorization → Handler.
        // The first registered behaviour wraps the rest, so the order below matters.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

        return services;
    }
}
