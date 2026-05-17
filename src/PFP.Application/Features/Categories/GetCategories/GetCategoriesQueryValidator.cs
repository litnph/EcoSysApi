using FluentValidation;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Categories.GetCategories;

/// <summary>FluentValidation rules for <see cref="GetCategoriesQuery"/>.</summary>
public sealed class GetCategoriesQueryValidator : AbstractValidator<GetCategoriesQuery>
{
    /// <summary>Registers validation rules.</summary>
    public GetCategoriesQueryValidator()
    {
        RuleFor(x => x.SmoduleId).NotEmpty();
        RuleFor(x => x.Kind).IsInEnum();
    }
}
