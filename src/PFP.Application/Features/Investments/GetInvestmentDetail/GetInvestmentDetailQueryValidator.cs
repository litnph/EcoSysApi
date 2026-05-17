using FluentValidation;

namespace PFP.Application.Features.Investments.GetInvestmentDetail;

public sealed class GetInvestmentDetailQueryValidator : AbstractValidator<GetInvestmentDetailQuery>
{
    public GetInvestmentDetailQueryValidator() => RuleFor(x => x.Id).NotEmpty();
}
