using FluentValidation;
using MediatR;

namespace PFP.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline stage that executes every <see cref="IValidator{T}"/> registered for the request
/// before the handler runs (spec §2.3).
/// </summary>
/// <typeparam name="TRequest">Incoming command or query.</typeparam>
/// <typeparam name="TResponse">Handler return type.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>Creates the behaviour.</summary>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next().ConfigureAwait(false);

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)))
            .ConfigureAwait(false))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next().ConfigureAwait(false);
    }
}
