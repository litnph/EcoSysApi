using FluentValidation;

namespace PFP.Application.Features.Savings.GetSavings;

public sealed class GetSavingsQueryValidator : AbstractValidator<GetSavingsQuery>
{
    public GetSavingsQueryValidator() => RuleFor(x => x.SmoduleId).NotEmpty();
}
