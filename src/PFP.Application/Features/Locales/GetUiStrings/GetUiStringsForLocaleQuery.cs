using FluentValidation;
using MediatR;

namespace PFP.Application.Features.Locales.GetUiStrings;

/// <summary>Validated by <see cref="GetUiStringsForLocaleQueryValidator"/>.</summary>
public sealed record GetUiStringsForLocaleQuery(string LocaleCode) : IRequest<IReadOnlyDictionary<string, string>>;

/// <summary>Requires an existing active locale code.</summary>
public sealed class GetUiStringsForLocaleQueryValidator : AbstractValidator<GetUiStringsForLocaleQuery>
{
    /// <summary>Creates the validator.</summary>
    public GetUiStringsForLocaleQueryValidator()
    {
        RuleFor(x => x.LocaleCode)
            .NotEmpty()
            .MaximumLength(20);
    }
}
