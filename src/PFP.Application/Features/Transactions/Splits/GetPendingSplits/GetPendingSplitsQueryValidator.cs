using FluentValidation;

namespace PFP.Application.Features.Transactions.Splits.GetPendingSplits;

/// <summary>Validates <see cref="GetPendingSplitsQuery"/>.</summary>
public sealed class GetPendingSplitsQueryValidator : AbstractValidator<GetPendingSplitsQuery>
{
    public GetPendingSplitsQueryValidator()
    {
}
}
