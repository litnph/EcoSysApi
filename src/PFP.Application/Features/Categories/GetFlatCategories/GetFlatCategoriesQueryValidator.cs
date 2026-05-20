using FluentValidation;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Categories.GetFlatCategories;

/// <summary>FluentValidation rules for <see cref="GetFlatCategoriesQuery"/>.</summary>
public sealed class GetFlatCategoriesQueryValidator : AbstractValidator<GetFlatCategoriesQuery>
{
    /// <summary>Registers validation rules.</summary>
    public GetFlatCategoriesQueryValidator()
    {
RuleFor(x => x.Kind).IsInEnum();
    }
}
