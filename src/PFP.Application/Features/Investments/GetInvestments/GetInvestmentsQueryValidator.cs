using FluentValidation;

namespace PFP.Application.Features.Investments.GetInvestments;

public sealed class GetInvestmentsQueryValidator : AbstractValidator<GetInvestmentsQuery>
{
    public GetInvestmentsQueryValidator() => RuleFor(x => x.SmoduleId).NotEmpty();
}
