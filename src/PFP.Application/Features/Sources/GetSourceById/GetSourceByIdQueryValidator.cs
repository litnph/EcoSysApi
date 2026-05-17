using FluentValidation;

namespace PFP.Application.Features.Sources.GetSourceById;

/// <summary>FluentValidation rules for <see cref="GetSourceByIdQuery"/>.</summary>
public sealed class GetSourceByIdQueryValidator : AbstractValidator<GetSourceByIdQuery>
{
    /// <summary>Registers query rules.</summary>
    public GetSourceByIdQueryValidator() =>
        RuleFor(x => x.Id).NotEmpty();
}
