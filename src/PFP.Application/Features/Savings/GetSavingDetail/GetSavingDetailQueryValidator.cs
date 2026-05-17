using FluentValidation;

namespace PFP.Application.Features.Savings.GetSavingDetail;

public sealed class GetSavingDetailQueryValidator : AbstractValidator<GetSavingDetailQuery>
{
    public GetSavingDetailQueryValidator() => RuleFor(x => x.Id).NotEmpty();
}
